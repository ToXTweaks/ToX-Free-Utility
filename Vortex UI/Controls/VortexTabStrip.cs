using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexTabStrip : Control
    {
        // Tab items collection
        private List<TabItem> _tabItems = new List<TabItem>();

        // Properties
        private int _selectedIndex = 0;
        private Color _selectedColor = Color.White;
        private Color _unselectedColor = Color.Gray;
        private Color _separatorColor = Color.FromArgb(64, 158, 255);
        private Color _bottomSeparatorColor = Color.FromArgb(65, 65, 56);
        private int _tabHeight = 40;
        private int _separatorHeight = 3;
        private int _tabPadding = 10;
        private int _bottomSeparatorHeight = 1;

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The color used for selected tabs")]
        public Color SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The color used for unselected tabs")]
        public Color UnselectedColor
        {
            get => _unselectedColor;
            set { _unselectedColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The color used for active tab separator")]
        public Color SeparatorColor
        {
            get => _separatorColor;
            set { _separatorColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The color used for bottom separator line")]
        public Color BottomSeparatorColor
        {
            get => _bottomSeparatorColor;
            set { _bottomSeparatorColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The height of tabs")]
        public int TabHeight
        {
            get => _tabHeight;
            set { _tabHeight = Math.Max(value, 1); Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The height of the active tab separator")]
        public int SeparatorHeight
        {
            get => _separatorHeight;
            set { _separatorHeight = Math.Max(value, 1); Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The height of the bottom separator line")]
        public int BottomSeparatorHeight
        {
            get => _bottomSeparatorHeight;
            set { _bottomSeparatorHeight = Math.Max(value, 0); Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The padding around tab text")]
        public int TabPadding
        {
            get => _tabPadding;
            set { _tabPadding = Math.Max(value, 0); Invalidate(); }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("The currently selected tab index")]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value >= 0 && value < TabPages.Count)
                {
                    _selectedIndex = value;
                    OnSelectedIndexChanged(EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        // Collection of tab pages
        [Browsable(true)]
        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(TabPageCollectionEditor), typeof(UITypeEditor))]
        public TabPageCollection TabPages { get; } = new TabPageCollection();

        // Events
        public event EventHandler SelectedIndexChanged;

        // ✅ Event جديد للبنل اللي ظهر
        public event EventHandler<Panel> PanelShown;

        // Constructor
        public VortexTabStrip()
        {
            SetStyle(ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            try { Font = new Font("Bahnschrift", 10F, FontStyle.Regular); }
            catch { Font = new Font("Segoe UI", 10F, FontStyle.Regular); }

            Size = new Size(400, 40);

            TabPages.TabPagesChanged += (sender, e) => RefreshControl();
        }

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);
            BringSelectedPanelToFront();
        }

        private void BringSelectedPanelToFront()
        {
            if (_selectedIndex >= 0 && _selectedIndex < TabPages.Count)
            {
                if (TabPages[_selectedIndex].AssociatedPanel != null)
                {
                    var panel = TabPages[_selectedIndex].AssociatedPanel;
                    panel.BringToFront();

                    // ✅ نادينا الـ Event لما البانل يبان
                    PanelShown?.Invoke(this, panel);
                }
            }
        }

        private void RefreshControl()
        {
            if (TabPages.Count > 0 && _selectedIndex >= TabPages.Count)
                _selectedIndex = TabPages.Count - 1;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int x = _tabPadding;
            for (int i = 0; i < TabPages.Count; i++)
            {
                TabPage tab = TabPages[i];
                bool isSelected = (i == _selectedIndex);
                Color textColor = isSelected ? _selectedColor : _unselectedColor;

                Size textSize = TextRenderer.MeasureText(tab.Title, Font, Size.Empty, TextFormatFlags.NoPadding);
                int tabWidth = textSize.Width + _tabPadding * 2;

                Rectangle tabRect = new Rectangle(x, 0, tabWidth, _tabHeight);
                TextRenderer.DrawText(g, tab.Title, Font, tabRect, textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding | TextFormatFlags.PreserveGraphicsClipping);

                if (isSelected && _separatorHeight > 0)
                {
                    using (Brush brush = new SolidBrush(_separatorColor))
                    using (GraphicsPath path = CreateRoundedRectangle(
                        x, _tabHeight - _separatorHeight, tabWidth, _separatorHeight,
                        Math.Min(_separatorHeight / 2, 1)))
                    {
                        g.FillPath(brush, path);
                    }
                }

                x += tabWidth;
            }

            if (_bottomSeparatorHeight > 0)
            {
                using (Brush brush = new SolidBrush(_bottomSeparatorColor))
                {
                    g.FillRectangle(brush, 0, _tabHeight - _bottomSeparatorHeight, Width, _bottomSeparatorHeight);
                }
            }
        }

        private GraphicsPath CreateRoundedRectangle(int x, int y, int width, int height, int radius)
        {
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;
            radius = Math.Min(radius, Math.Min(height / 2, width / 2));

            if (radius <= 0)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddRectangle(new Rectangle(x, y, width, height));
                return path;
            }

            GraphicsPath roundedPath = new GraphicsPath();
            roundedPath.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            roundedPath.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
            roundedPath.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
            roundedPath.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
            roundedPath.CloseFigure();
            return roundedPath;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int x = _tabPadding;
            for (int i = 0; i < TabPages.Count; i++)
            {
                TabPage tab = TabPages[i];
                Size textSize = TextRenderer.MeasureText(tab.Title, Font, Size.Empty, TextFormatFlags.NoPadding);
                int tabWidth = textSize.Width + _tabPadding * 2;

                Rectangle tabRect = new Rectangle(x, 0, tabWidth, _tabHeight);

                if (tabRect.Contains(e.Location))
                {
                    SelectedIndex = i;
                    break;
                }

                x += tabWidth;
            }
        }

        public class TabPageCollection : List<TabPage>
        {
            public event EventHandler TabPagesChanged;
            public new void Add(TabPage item) { base.Add(item); OnTabPagesChanged(); }
            public new void Remove(TabPage item) { base.Remove(item); OnTabPagesChanged(); }
            public new void Clear() { base.Clear(); OnTabPagesChanged(); }
            public new TabPage this[int index]
            {
                get => base[index];
                set { base[index] = value; OnTabPagesChanged(); }
            }
            private void OnTabPagesChanged() => TabPagesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TabPage : INotifyPropertyChanged
    {
        private string _title = "New Tab";
        private Panel _associatedPanel;

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The title of the tab")]
        public string Title
        {
            get => _title;
            set { if (_title != value) { _title = value; OnPropertyChanged(nameof(Title)); } }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("The panel associated with this tab")]
        public Panel AssociatedPanel
        {
            get => _associatedPanel;
            set { if (_associatedPanel != value) { _associatedPanel = value; OnPropertyChanged(nameof(AssociatedPanel)); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public override string ToString() => Title;
    }

    public class TabPageCollectionEditor : CollectionEditor
    {
        public TabPageCollectionEditor(Type type) : base(type) { }
        protected override Type[] CreateNewItemTypes() => new Type[] { typeof(TabPage) };
    }

    internal class TabItem
    {
        public string Text { get; set; }
        public Panel AssociatedPanel { get; set; }
    }
}
