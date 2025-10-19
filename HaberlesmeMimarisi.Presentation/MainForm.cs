using HaberlesmeMimarisi.App.Discovery;
using HaberlesmeMimarisi.App.Messaging;
using HaberlesmeMimarisi.Core.Messaging;
using HaberlesmeMimarisi.Domain.Messages;
using HaberlesmeMimarisi.Domain.Parsing;
using HaberlesmeMimarisi.Domain.Repositories;
using HaberlesmeMimarisi.Infrastructure.Transport;
using HaberlesmeMimarisi.Repository.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HaberlesmeMimarisi.Presentation
{
    public partial class MainForm : Form
    {
        private readonly BindingList<MessageRowViewModel> _rows = new BindingList<MessageRowViewModel>();
        private IMessageTransport _transport;
        private IMessageClient _client;
        private ICardIdDiscovery _cardDisc;
        private byte _cardId;
        FramedMessageClient _framedClient;
        private CancellationTokenSource _cts;
        private volatile bool _busy;

        public MainForm()
        {
            InitializeComponent();
            dgv.AutoGenerateColumns = false;
            dgv.DataSource = _rows;
            BuildColumns();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Demo transport (Fake). Switch to SerialPortTransport for real hardware.
            //_transport = new FakeLoopbackTransport(cardId: 0x3A);
            _transport = new FakeLoopbackTransport(cardId: 0x3A, true, 34);
            // _transport = new SerialPortTransport("COM5", 115200);
            _transport.Open();

            //_client = new MessageClient(_transport);  //çalışıyor
            _cardDisc = new CardIdDiscovery(_transport);
            _cardId = _cardDisc.DiscoverCardId();

            IFrameReader fr = new FixedLengthFrameReader(_transport, frameLength: 4);
            IRxMessageParser parser = new Fixed4ByteRxMessageParser();
            _framedClient = new FramedMessageClient(_transport, fr, parser);
            
           // Load definitions via repository
           IMessageDefinitionRepository repo = new JsonMessageDefinitionRepository();//works for in-memory message list

            
             /*
            // JSON’dan yükleme şeklindeki aşağıdaki kod bloğu da çalışıyor
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleMessageList.json");
            jsonPath = @"C:\projeler\CihazKutuphanesi\HaberlesmeMimarileri\HaberlesmeMimarisi\HaberlesmeMimarisi.Repository\bin\Debug\SampleMessageList.json";
            //@C:\projeler\CihazKutuphanesi\HaberlesmeMimarileri\HaberlesmeMimarisi\HaberlesmeMimarisi.Repository\bin\Debug\SampleMessageList.json
            //C:\projeler\CihazKutuphanesi\HaberlesmeMimarileri\HaberlesmeMimarisi\HaberlesmeMimarisi.Repository\bin\Debug\SampleMessageList.json            
            IMessageDefinitionRepository repo = new FileBackedMessageDefinitionRepository(jsonPath); */
                    

            foreach (var def in repo.GetAll())
            {
                _rows.Add(new MessageRowViewModel
                {
                    Definition = def,
                    TxMessageName = def.TxName,
                    RxMessageName = def.RxName,
                    TxMessageId = HaberlesmeMimarisi.Core.Utils.Bytes.Hex(def.TxId)
                });
            }
        }

        private void BuildColumns()
        {
            dgv.Columns.Clear();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TxMessage Name", DataPropertyName = "TxMessageName", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TX Mesaj ID", DataPropertyName = "TxMessageId", Width = 90 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TxMessage", DataPropertyName = "TxMessageHex", Width = 150 });

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rx Message Name", DataPropertyName = "RxMessageName", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rx Mesaj ID", DataPropertyName = "RxMessageId", Width = 90 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "RX Message", DataPropertyName = "RxMessageHex", Width = 150 });

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "RxData", DataPropertyName = "RxData", Width = 90 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Alt Limit", DataPropertyName = "AltLimit", Width = 90 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Anlamlı RxData", DataPropertyName = "AnlamliRxData", Width = 120 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Üst Limit", DataPropertyName = "UstLimit", Width = 90 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Geçti-Kaldı", DataPropertyName = "GectiKaldi", Width = 100 });

            var btn = new DataGridViewButtonColumn
            {
                HeaderText = "Gönder",
                Text = "Gönder",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            dgv.Columns.Add(btn);

            dgv.CellContentClick += Dgv_CellContentClick;
        }

        private async void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!(dgv.Columns[e.ColumnIndex] is DataGridViewButtonColumn)) return;
            if (_busy) return;

            _cts = new CancellationTokenSource();
            try
            {
                _busy = true;
                dgv.Enabled = false;
                var row = (MessageRowViewModel)dgv.Rows[e.RowIndex].DataBoundItem;
                await SendOneAsync(row, _cts.Token);
            }
            finally
            {
                dgv.Enabled = true;
                _busy = false;
                _cts.Dispose();
                _cts = null;
            }
        }

        private async Task SendOneAsync(MessageRowViewModel row, CancellationToken ct)
        {
            var def = row.Definition;
            var tx = new TxMessage(def.TxId, _cardId, row.PendingTxData);

            try
            {
                var rx = await Task.Run(() =>
                {
                    ct.ThrowIfCancellationRequested();
                    return _framedClient.Request(tx, timeoutMs: 200);
                }, ct);

                var eval = def.Evaluator.Evaluate(def, tx, rx);
                row.ApplyEvaluation(tx, rx, eval);
                row.RxMessageName = def.RxName;
                dgv.Refresh();
            }
            catch (OperationCanceledException)
            {
                row.AnlamliRxData = "İptal edildi";
                row.GectiKaldi = "";
                dgv.Refresh();
            }
            catch (Exception ex)
            {
                row.AnlamliRxData = ex.Message;
                row.GectiKaldi = "Kaldı";
                dgv.Refresh();
            }
        }

        private async void btnSendAll_Click(object sender, EventArgs e)
        {
            if (_busy) return;

            _cts = new CancellationTokenSource();
            try
            {
                _busy = true;
                dgv.Enabled = false;
                btnSendAll.Enabled = false;
                btnCancel.Enabled = true;

                foreach (var item in _rows)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    await SendOneAsync(item, _cts.Token);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                dgv.Enabled = true;
                btnSendAll.Enabled = true;
                btnCancel.Enabled = false;
                _busy = false;
                _cts.Dispose();
                _cts = null;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _transport?.Dispose();
        }

        private void dgv_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {

            //satır bazlı renklendirme örneği
            /*if (e.RowIndex >= 0 && e.RowIndex < dgv.Rows.Count)
            {
                var row = dgv.Rows[e.RowIndex];
                var passFail = row.Cells[0x0A]?.Value?.ToString();

                if (!string.IsNullOrEmpty(passFail))
                {
                    if (passFail.Contains("Geçti"))
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                    }
                    else if (passFail.Contains("Kaldı"))
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                    }
                }
            }  */
        }

        private void dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Kolonu "Geçti-Kaldı" olan hücrelerde renklendirme
            var col = dgv.Columns[e.ColumnIndex];
            if (col.DataPropertyName == "GectiKaldi" || col.HeaderText == "Geçti-Kaldı")
            {
                string text = (e.Value ?? "").ToString().Trim();

                // Önce varsayılana dön (önceki stiller kalmasın)
                e.CellStyle.BackColor = dgv.DefaultCellStyle.BackColor;
                e.CellStyle.ForeColor = dgv.DefaultCellStyle.ForeColor;
                e.CellStyle.SelectionBackColor = dgv.DefaultCellStyle.SelectionBackColor;
                e.CellStyle.SelectionForeColor = dgv.DefaultCellStyle.SelectionForeColor;

                if (text.Equals("Geçti", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = System.Drawing.Color.LightGreen;
                    e.CellStyle.ForeColor = System.Drawing.Color.Black;
                    e.CellStyle.SelectionBackColor = System.Drawing.Color.DarkSeaGreen;
                    e.CellStyle.SelectionForeColor = System.Drawing.Color.Black;
                }
                else if (text.Equals("Kaldı", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = System.Drawing.Color.MistyRose;
                    e.CellStyle.ForeColor = System.Drawing.Color.DarkRed;
                    e.CellStyle.SelectionBackColor = System.Drawing.Color.IndianRed;
                    e.CellStyle.SelectionForeColor = System.Drawing.Color.White;
                }
            }
        }
    }
}
