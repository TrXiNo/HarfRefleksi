using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace HarfPratik
{
    public partial class Form1 : Form
    {
        private const string Harfler =
            "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZ";

        private readonly string[] klavyeSatirlari =
        {
            "QWERTYUIOPĞÜ",
            "ASDFGHJKLŞİ",
            "ZXCVBNMÖÇ"
        };

        // Krem ve kahverengi renk paleti
        private readonly Color arkaPlanRengi =
            Color.FromArgb(245, 237, 220);

        private readonly Color kartRengi =
            Color.FromArgb(225, 210, 184);

        private readonly Color tusRengi =
            Color.FromArgb(205, 184, 151);

        private readonly Color tusYaziRengi =
            Color.FromArgb(75, 54, 33);

        private readonly Color koyuKahverengi =
            Color.FromArgb(91, 64, 38);

        private readonly Color dogruRengi =
            Color.FromArgb(116, 130, 89);

        private readonly Color yanlisRengi =
            Color.FromArgb(181, 101, 78);

        private readonly Random rastgele = new();
        private readonly Stopwatch kronometre = new();
        private readonly CultureInfo turkce = new("tr-TR");
        private readonly Dictionary<char, YuvarlakLabel> tuslar = new();

        private Label lblHarf = null!;
        private Label lblBilgi = null!;
        private Label lblDogru = null!;
        private Label lblYanlis = null!;
        private Label lblDogruluk = null!;
        private Label lblSure = null!;

        private YuvarlakPanel harfAlani = null!;
        private Button btnSifirla = null!;
        private System.Windows.Forms.Timer zamanlayici = null!;

        private char mevcutHarf;
        private int dogruSayisi;
        private int yanlisSayisi;

        public Form1()
        {
            InitializeComponent();

            ArayuzuHazirla();
            YeniHarfGetir();

            KeyPreview = true;
            KeyPress += Form1_KeyPress;
            KeyDown += Form1_KeyDown;

            Shown += (_, _) =>
            {
                ActiveControl = null;
                Focus();
            };
        }

        private void ArayuzuHazirla()
        {
            Text = "Harf Refleksi - 10 Parmak Klavye Pratiği";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1100, 760);
            MinimumSize = new Size(900, 700);

            BackColor = arkaPlanRengi;
            ForeColor = koyuKahverengi;

            TableLayoutPanel anaPanel = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(35, 18, 35, 18),
                BackColor = arkaPlanRengi
            };

            anaPanel.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 65));

            anaPanel.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 205));

            anaPanel.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 42));

            anaPanel.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            anaPanel.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 70));

            anaPanel.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 50));

            Label lblBaslik = new()
            {
                Text = "10 PARMAK HARF PRATİĞİ",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(
                    "Segoe UI",
                    23,
                    FontStyle.Bold
                ),
                ForeColor = koyuKahverengi
            };

            harfAlani = new YuvarlakPanel
            {
                Size = new Size(320, 190),
                Anchor = AnchorStyles.None,
                BackColor = kartRengi,
                Radius = 25
            };

            lblHarf = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(
                    "Segoe UI",
                    88,
                    FontStyle.Bold
                ),
                ForeColor = koyuKahverengi,
                BackColor = Color.Transparent
            };

            harfAlani.Controls.Add(lblHarf);

            lblBilgi = new Label
            {
                Text = "Ekrandaki harfin tuşuna bas",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(120, 89, 58)
            };

            Control klavye = KlavyeOlustur();

            TableLayoutPanel istatistikPaneli = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            for (int i = 0; i < 4; i++)
            {
                istatistikPaneli.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 25)
                );
            }

            lblDogru = IstatistikLabeliOlustur("Doğru", "0");
            lblYanlis = IstatistikLabeliOlustur("Yanlış", "0");
            lblDogruluk = IstatistikLabeliOlustur(
                "Doğruluk",
                "%100"
            );
            lblSure = IstatistikLabeliOlustur("Süre", "00:00");

            istatistikPaneli.Controls.Add(lblDogru, 0, 0);
            istatistikPaneli.Controls.Add(lblYanlis, 1, 0);
            istatistikPaneli.Controls.Add(lblDogruluk, 2, 0);
            istatistikPaneli.Controls.Add(lblSure, 3, 0);

            btnSifirla = new Button
            {
                Text = "Yeniden Başlat (Esc)",
                Size = new Size(200, 40),
                Anchor = AnchorStyles.None,
                Font = new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold
                ),
                BackColor = koyuKahverengi,
                ForeColor = Color.FromArgb(250, 243, 229),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TabStop = false
            };

            btnSifirla.FlatAppearance.BorderSize = 0;
            btnSifirla.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(113, 80, 49);

            btnSifirla.Click += (_, _) => OyunuSifirla();

            zamanlayici = new System.Windows.Forms.Timer
            {
                Interval = 250
            };

            zamanlayici.Tick += (_, _) => SureyiGuncelle();
            zamanlayici.Start();

            anaPanel.Controls.Add(lblBaslik, 0, 0);
            anaPanel.Controls.Add(harfAlani, 0, 1);
            anaPanel.Controls.Add(lblBilgi, 0, 2);
            anaPanel.Controls.Add(klavye, 0, 3);
            anaPanel.Controls.Add(istatistikPaneli, 0, 4);
            anaPanel.Controls.Add(btnSifirla, 0, 5);

            Controls.Add(anaPanel);
        }

        private Control KlavyeOlustur()
        {
            TableLayoutPanel klavyePaneli = new()
            {
                Anchor = AnchorStyles.None,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            for (int i = 0; i < 4; i++)
            {
                klavyePaneli.RowStyles.Add(
                    new RowStyle(SizeType.AutoSize)
                );
            }

            for (int i = 0; i < klavyeSatirlari.Length; i++)
            {
                FlowLayoutPanel satir =
                    KlavyeSatiriOlustur(klavyeSatirlari[i]);

                if (i == 1)
                    satir.Margin = new Padding(35, 3, 0, 3);
                else if (i == 2)
                    satir.Margin = new Padding(78, 3, 0, 3);
                else
                    satir.Margin = new Padding(0, 3, 0, 3);

                klavyePaneli.Controls.Add(satir, 0, i);
            }

            // Görseldeki uzun boşluk tuşu
            FlowLayoutPanel boslukSatiri = new()
            {
                Anchor = AnchorStyles.None,
                AutoSize = true,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 5, 0, 0)
            };

            YuvarlakLabel boslukTusu = new()
            {
                Text = "",
                Size = new Size(460, 54),
                BackColor = tusRengi,
                Radius = 15,
                Margin = new Padding(0),
                Cursor = Cursors.Default
            };

            boslukSatiri.Controls.Add(boslukTusu);
            klavyePaneli.Controls.Add(boslukSatiri, 0, 3);

            return klavyePaneli;
        }

        private FlowLayoutPanel KlavyeSatiriOlustur(
            string harfSatiri)
        {
            FlowLayoutPanel satir = new()
            {
                Anchor = AnchorStyles.None,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };

            foreach (char harf in harfSatiri)
            {
                YuvarlakLabel tus = new()
                {
                    Text = harf.ToString(),
                    Size = new Size(66, 62),
                    Margin = new Padding(4),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font(
                        "Consolas",
                        19,
                        FontStyle.Bold
                    ),
                    ForeColor = tusYaziRengi,
                    BackColor = tusRengi,
                    Radius = 14
                };

                tuslar.Add(harf, tus);
                satir.Controls.Add(tus);
            }

            return satir;
        }

        private Label IstatistikLabeliOlustur(
            string baslik,
            string deger)
        {
            return new Label
            {
                Text = $"{baslik}\n{deger}",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(
                    "Segoe UI",
                    11,
                    FontStyle.Bold
                ),
                ForeColor = koyuKahverengi
            };
        }

        private void Form1_KeyPress(
            object? sender,
            KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar))
                return;

            char basilanHarf =
                char.ToUpper(e.KeyChar, turkce);

            if (!Harfler.Contains(basilanHarf))
                return;

            if (!kronometre.IsRunning)
                kronometre.Start();

            bool dogruMu = basilanHarf == mevcutHarf;

            if (dogruMu)
            {
                dogruSayisi++;

                lblBilgi.Text = "Doğru!";
                lblBilgi.ForeColor = dogruRengi;

                TusRenginiDegistir(
                    basilanHarf,
                    dogruRengi
                );

                YeniHarfGetir();
            }
            else
            {
                yanlisSayisi++;

                lblBilgi.Text =
                    $"Yanlış! {mevcutHarf} tuşuna basmalısın.";

                lblBilgi.ForeColor = yanlisRengi;

                TusRenginiDegistir(
                    basilanHarf,
                    yanlisRengi
                );
            }

            IstatistikleriGuncelle();
            e.Handled = true;
        }

        private void Form1_KeyDown(
            object? sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                OyunuSifirla();
                e.SuppressKeyPress = true;
            }
        }

        private async void TusRenginiDegistir(
            char harf,
            Color renk)
        {
            if (!tuslar.TryGetValue(
                    harf,
                    out YuvarlakLabel? tus))
            {
                return;
            }

            tus.BackColor = renk;
            tus.ForeColor = Color.White;
            tus.Invalidate();

            await Task.Delay(280);

            if (IsDisposed || tus.IsDisposed)
                return;

            tus.BackColor = tusRengi;
            tus.ForeColor = tusYaziRengi;
            tus.Invalidate();
        }

        private void YeniHarfGetir()
        {
            char yeniHarf;

            do
            {
                yeniHarf =
                    Harfler[rastgele.Next(Harfler.Length)];
            }
            while (yeniHarf == mevcutHarf);

            mevcutHarf = yeniHarf;
            lblHarf.Text = mevcutHarf.ToString();
            lblHarf.ForeColor = koyuKahverengi;
        }

        private void IstatistikleriGuncelle()
        {
            int toplam = dogruSayisi + yanlisSayisi;

            int dogruluk = toplam == 0
                ? 100
                : (int)Math.Round(
                    dogruSayisi * 100.0 / toplam);

            lblDogru.Text = $"Doğru\n{dogruSayisi}";
            lblYanlis.Text = $"Yanlış\n{yanlisSayisi}";
            lblDogruluk.Text = $"Doğruluk\n%{dogruluk}";

            SureyiGuncelle();
        }

        private void SureyiGuncelle()
        {
            lblSure.Text =
                $"Süre\n{kronometre.Elapsed:mm\\:ss}";
        }

        private void OyunuSifirla()
        {
            dogruSayisi = 0;
            yanlisSayisi = 0;

            kronometre.Reset();

            foreach (YuvarlakLabel tus in tuslar.Values)
            {
                tus.BackColor = tusRengi;
                tus.ForeColor = tusYaziRengi;
                tus.Invalidate();
            }

            IstatistikleriGuncelle();
            YeniHarfGetir();

            lblBilgi.Text = "Yeni pratik başladı";
            lblBilgi.ForeColor =
                Color.FromArgb(120, 89, 58);

            ActiveControl = null;
            Focus();
        }
    }

    // Yuvarlatılmış klavye tuşları
    public class YuvarlakLabel : Label
    {
        public int Radius { get; set; } = 14;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            YuvarlakSekilOlustur();
        }

        protected override void OnPaint(
            PaintEventArgs e)
        {
            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            using GraphicsPath yol =
                YuvarlakDortgen(
                    ClientRectangle,
                    Radius
                );

            using SolidBrush firca =
                new SolidBrush(BackColor);

            e.Graphics.FillPath(firca, yol);

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                ClientRectangle,
                ForeColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter
            );
        }

        private void YuvarlakSekilOlustur()
        {
            if (Width <= 0 || Height <= 0)
                return;

            using GraphicsPath yol =
                YuvarlakDortgen(
                    ClientRectangle,
                    Radius
                );

            Region = new Region(yol);
        }

        private static GraphicsPath YuvarlakDortgen(
            Rectangle alan,
            int radius)
        {
            GraphicsPath yol = new();

            int cap = radius * 2;

            Rectangle duzeltilmisAlan = new(
                alan.X,
                alan.Y,
                Math.Max(1, alan.Width - 1),
                Math.Max(1, alan.Height - 1)
            );

            yol.AddArc(
                duzeltilmisAlan.Left,
                duzeltilmisAlan.Top,
                cap,
                cap,
                180,
                90
            );

            yol.AddArc(
                duzeltilmisAlan.Right - cap,
                duzeltilmisAlan.Top,
                cap,
                cap,
                270,
                90
            );

            yol.AddArc(
                duzeltilmisAlan.Right - cap,
                duzeltilmisAlan.Bottom - cap,
                cap,
                cap,
                0,
                90
            );

            yol.AddArc(
                duzeltilmisAlan.Left,
                duzeltilmisAlan.Bottom - cap,
                cap,
                cap,
                90,
                90
            );

            yol.CloseFigure();

            return yol;
        }
    }

    // Büyük harfin bulunduğu yuvarlak kart
    public class YuvarlakPanel : Panel
    {
        public int Radius { get; set; } = 25;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (Width <= 0 || Height <= 0)
                return;

            using GraphicsPath yol = SekilOlustur();

            Region = new Region(yol);
            Invalidate();
        }

        protected override void OnPaint(
            PaintEventArgs e)
        {
            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            using GraphicsPath yol = SekilOlustur();
            using SolidBrush firca =
                new SolidBrush(BackColor);

            e.Graphics.FillPath(firca, yol);
        }

        private GraphicsPath SekilOlustur()
        {
            GraphicsPath yol = new();

            int cap = Radius * 2;

            Rectangle alan = new(
                0,
                0,
                Math.Max(1, Width - 1),
                Math.Max(1, Height - 1)
            );

            yol.AddArc(
                alan.Left,
                alan.Top,
                cap,
                cap,
                180,
                90
            );

            yol.AddArc(
                alan.Right - cap,
                alan.Top,
                cap,
                cap,
                270,
                90
            );

            yol.AddArc(
                alan.Right - cap,
                alan.Bottom - cap,
                cap,
                cap,
                0,
                90
            );

            yol.AddArc(
                alan.Left,
                alan.Bottom - cap,
                cap,
                cap,
                90,
                90
            );

            yol.CloseFigure();

            return yol;
        }
    }
}