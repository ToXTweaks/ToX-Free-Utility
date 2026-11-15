using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Vortex.Ui
{
    public enum IconVisibility { Always, SelectedOnly, Never }
    public enum PageSelectorOrientation { Horizontal, Vertical }

    [ToolboxItem(true)]
    public class VortexPageSelector : Control
    {
        Bitmap _bgCache;
        bool _bgCacheValid;
        Control _prevParent;

        PageButtonCollection _buttons;
        int _selectedIndex;
        int _selectedChildIndex = -1;
        int _hoveredIndex = -1;
        int _hoveredChildIndex = -1;

        Color _selectedColor = Color.FromArgb(255, 120, 0);
        Color _unselectedColor = Color.FromArgb(60, 60, 64);
        Color _hoverColor = Color.FromArgb(80, 80, 84);
        Color _textColor = Color.White;
        Color _selectedTextColor = Color.Black;
        Color _selectedIconColor = Color.Empty;
        Color _unselectedIconColor = Color.Empty;

        int _buttonHeight = 44;
        int _buttonRadius = 20;
        int _buttonSpacing = 0;
        int _iconSize = 24;
        int _iconSpacing = 5;
        int _padding = 10;
        int _buttonPadding = 22;
        int _childIndent = 18;
        int _childButtonHeight = 34;
        int _childButtonSpacing = 2;
        int _childButtonPadding = 14;
        int _expandArrowSize = 8;

        PageSelectorOrientation _orientation = PageSelectorOrientation.Horizontal;

        Timer _animationTimer;
        float _animationProgress = 1f;
        Rectangle _animationStartBounds;
        Rectangle _animationEndBounds;
        int _animationTargetIndex;
        int _animationTargetChildIndex = -1;
        float _animationSpeed = 0.25f;

        IconVisibility _iconVisibility = IconVisibility.Always;

        [Category("Appearance"), Description("Color of the selected button")]
        public Color SelectedColor { get => _selectedColor; set { _selectedColor = value; Invalidate(); } }

        [Category("Appearance"), Description("Color of unselected buttons")]
        public Color UnselectedColor { get => _unselectedColor; set { _unselectedColor = value; Invalidate(); } }

        [Category("Appearance"), Description("Color when hovering over buttons")]
        public Color HoverColor { get => _hoverColor; set { _hoverColor = value; Invalidate(); } }

        [Category("Appearance"), Description("Text color for buttons")]
        public Color TextColor { get => _textColor; set { _textColor = value; Invalidate(); } }

        [Category("Appearance"), Description("Text color for selected button")]
        public Color SelectedTextColor { get => _selectedTextColor; set { _selectedTextColor = value; Invalidate(); } }

        [Category("Appearance"), DefaultValue(typeof(Color), "Empty"), Description("Optional icon tint color for the selected button. Empty uses the button IconTintColor.")]
        public Color SelectedIconColor { get => _selectedIconColor; set { _selectedIconColor = value; Invalidate(); } }

        [Category("Appearance"), DefaultValue(typeof(Color), "Empty"), Description("Optional icon tint color for unselected buttons. Empty uses each button IconTintColor.")]
        public Color UnselectedIconColor { get => _unselectedIconColor; set { _unselectedIconColor = value; Invalidate(); } }

        [Category("Appearance"), DefaultValue(45), Description("Height of buttons")]
        public int ButtonHeight { get => _buttonHeight; set { _buttonHeight = Math.Max(30, value); UpdateAutoSize(); Invalidate(); } }

        [Category("Appearance"), DefaultValue(22), Description("Border radius of buttons")]
        public int ButtonRadius { get => _buttonRadius; set { _buttonRadius = Math.Max(0, value); Invalidate(); } }

        [Category("Appearance"), DefaultValue(10), Description("Spacing between buttons")]
        public int ButtonSpacing { get => _buttonSpacing; set { _buttonSpacing = Math.Max(0, value); UpdateAutoSize(); Invalidate(); } }

        [Category("Appearance"), DefaultValue(24), Description("Size of icons")]
        public int IconSize { get => _iconSize; set { _iconSize = Math.Max(16, value); UpdateAutoSize(); Invalidate(); } }

        [Category("Appearance"), DefaultValue(5), Description("Spacing between icon and text")]
        public int IconSpacing { get => _iconSpacing; set { _iconSpacing = Math.Max(0, value); UpdateAutoSize(); Invalidate(); } }

        [Category("Appearance"), DefaultValue(20), Description("Horizontal padding inside buttons")]
        public int ButtonPadding { get => _buttonPadding; set { _buttonPadding = Math.Max(5, value); UpdateAutoSize(); Invalidate(); } }

        [Category("Layout"), DefaultValue(PageSelectorOrientation.Horizontal), Description("How the selector arranges root buttons")]
        public PageSelectorOrientation Orientation { get => _orientation; set { _orientation = value; UpdateAutoSize(); Invalidate(); } }

        [Category("Child Buttons"), DefaultValue(18), Description("Horizontal indent for child buttons")]
        public int ChildIndent { get => _childIndent; set { _childIndent = Math.Max(0, value); UpdateAutoSize(); Invalidate(); } }

        [Category("Child Buttons"), DefaultValue(34), Description("Height of child buttons")]
        public int ChildButtonHeight { get => _childButtonHeight; set { _childButtonHeight = Math.Max(20, value); UpdateAutoSize(); Invalidate(); } }

        [Category("Child Buttons"), DefaultValue(2), Description("Spacing between child buttons")]
        public int ChildButtonSpacing { get => _childButtonSpacing; set { _childButtonSpacing = Math.Max(0, value); UpdateAutoSize(); Invalidate(); } }

        [Category("Child Buttons"), DefaultValue(14), Description("Horizontal padding inside child buttons")]
        public int ChildButtonPadding { get => _childButtonPadding; set { _childButtonPadding = Math.Max(5, value); UpdateAutoSize(); Invalidate(); } }

        [DefaultValue(true)]
        public override bool AutoSize
        {
            get => base.AutoSize;
            set { base.AutoSize = value; UpdateAutoSize(); }
        }

        [Category("Animation"), DefaultValue(0.25f), Description("Animation speed")]
        public float AnimationSpeed { get => _animationSpeed; set { _animationSpeed = Math.Max(0.05f, Math.Min(1f, value)); } }

        [Category("Appearance"), DefaultValue(IconVisibility.Always), Description("Controls when icons are visible")]
        public IconVisibility IconVisibilityMode { get => _iconVisibility; set { _iconVisibility = value; UpdateAutoSize(); Invalidate(); } }

        [Category("Behavior"), Description("Currently selected page index")]
        public int SelectedIndex
        {
            get => _animationTimer.Enabled ? _animationTargetIndex : _selectedIndex;
            set
            {
                SelectButton(value, -1);
            }
        }

        [Category("Behavior"), DefaultValue(-1), Description("Currently selected child index for the selected root button. -1 means the root button itself is selected.")]
        public int SelectedChildIndex
        {
            get => _animationTimer.Enabled ? _animationTargetChildIndex : _selectedChildIndex;
            set
            {
                SelectButton(SelectedIndex, value);
            }
        }

        [Browsable(false)]
        public PageButton SelectedButton => GetButton(SelectedIndex, SelectedChildIndex);

        public void SelectButton(int parentIndex, int childIndex = -1)
        {
            if (!IsValidSelection(parentIndex, childIndex))
                return;

            if (_selectedIndex == parentIndex && _selectedChildIndex == childIndex)
                return;

            StartAnimation(parentIndex, childIndex);
            OnSelectionChanged();
        }

        public event EventHandler SelectedIndexChanged;
        public event EventHandler SelectedButtonChanged;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public PageButtonCollection Buttons => _buttons;

        public VortexPageSelector()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                BackColor = SystemColors.Control;
            else
                BackColor = Color.Transparent;

            AutoSize = true;
            Size = GetPreferredSize(Size.Empty);

            try { Font = new Font("Segoe UI", 10f, FontStyle.Regular); }
            catch { Font = SystemFonts.DefaultFont; }

            _buttons = new PageButtonCollection(this);
            _animationTimer = new Timer { Interval = 16 };
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (_buttons == null || _buttons.Count == 0)
                return new Size((_padding * 2) + 1, _buttonHeight + (_padding * 2));

            if (_orientation == PageSelectorOrientation.Vertical)
                return GetVerticalPreferredSize();

            return GetHorizontalPreferredSize();
        }

        Size GetHorizontalPreferredSize()
        {
            int totalWidth = _padding * 2;
            for (int i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                totalWidth += GetButtonWidth(button, IsLayoutSelected(i, -1), false);

                if (button != null && button.Expanded && button.Children.Count > 0)
                {
                    for (int childIndex = 0; childIndex < button.Children.Count; childIndex++)
                    {
                        totalWidth += _buttonSpacing;
                        totalWidth += GetButtonWidth(button.Children[childIndex], IsLayoutSelected(i, childIndex), true);
                    }
                }

                if (i < _buttons.Count - 1)
                    totalWidth += _buttonSpacing;
            }

            return new Size(Math.Max(1, totalWidth), _buttonHeight + (_padding * 2));
        }

        Size GetVerticalPreferredSize()
        {
            int maxWidth = 1;
            int totalHeight = _padding * 2;

            for (int i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                maxWidth = Math.Max(maxWidth, GetButtonWidth(button, IsLayoutSelected(i, -1), false));
                totalHeight += _buttonHeight;

                if (button != null && button.Expanded && button.Children.Count > 0)
                {
                    totalHeight += _childButtonSpacing + GetExpandedChildrenHeight(button);
                    for (int childIndex = 0; childIndex < button.Children.Count; childIndex++)
                        maxWidth = Math.Max(maxWidth, _childIndent + GetButtonWidth(button.Children[childIndex], IsLayoutSelected(i, childIndex), true));
                }

                if (i < _buttons.Count - 1)
                    totalHeight += _buttonSpacing;
            }

            return new Size(Math.Max(1, maxWidth + (_padding * 2)), Math.Max(1, totalHeight));
        }

        internal void UpdateAutoSize()
        {
            ValidateSelection();

            if (AutoSize)
                Size = GetPreferredSize(Size.Empty);

            _bgCacheValid = false;
        }

        int GetButtonWidth(PageButton button, bool isSelected, bool isChild = false)
        {
            int horizontalPadding = isChild ? _childButtonPadding : _buttonPadding;
            if (button == null)
                return horizontalPadding * 2;

            Size textSize = TextRenderer.MeasureText(
                button.Text ?? string.Empty,
                Font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

            int width = Math.Max(1, textSize.Width) + (horizontalPadding * 2);
            bool willShowIcon = button.Icon != null && ShouldShowIcon(isSelected);
            if (willShowIcon)
                width += _iconSize + _iconSpacing;

            if (!isChild && button.Children.Count > 0)
                width += _expandArrowSize + _iconSpacing;

            return width;
        }

        int GetExpandedChildrenHeight(PageButton button)
        {
            if (button == null || !button.Expanded || button.Children.Count == 0)
                return 0;

            return (button.Children.Count * _childButtonHeight) + ((button.Children.Count - 1) * _childButtonSpacing);
        }

        int GetLayoutSelectedIndex() => _animationTimer.Enabled ? _animationTargetIndex : _selectedIndex;
        int GetLayoutSelectedChildIndex() => _animationTimer.Enabled ? _animationTargetChildIndex : _selectedChildIndex;

        bool IsLayoutSelected(int parentIndex, int childIndex)
        {
            return parentIndex == GetLayoutSelectedIndex() && childIndex == GetLayoutSelectedChildIndex();
        }

        PageButton GetButton(int parentIndex, int childIndex)
        {
            if (_buttons == null || parentIndex < 0 || parentIndex >= _buttons.Count)
                return null;

            if (childIndex < 0)
                return _buttons[parentIndex];

            var parent = _buttons[parentIndex];
            if (parent == null || childIndex >= parent.Children.Count)
                return null;

            return parent.Children[childIndex];
        }

        void ValidateSelection()
        {
            if (_buttons == null || _buttons.Count == 0)
            {
                _selectedIndex = 0;
                _selectedChildIndex = -1;
                return;
            }

            if (_selectedIndex < 0)
                _selectedIndex = 0;
            if (_selectedIndex >= _buttons.Count)
                _selectedIndex = _buttons.Count - 1;

            var selectedParent = _buttons[_selectedIndex];
            if (_selectedChildIndex >= 0 && (selectedParent == null || !selectedParent.Expanded || _selectedChildIndex >= selectedParent.Children.Count))
                _selectedChildIndex = -1;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (_prevParent != null)
            {
                _prevParent.BackColorChanged -= Parent_RelevantChanged;
                _prevParent.BackgroundImageChanged -= Parent_RelevantChanged;
                _prevParent.SizeChanged -= Parent_RelevantChanged;
                _prevParent.ControlAdded -= Parent_RelevantChangedControls;
                _prevParent.ControlRemoved -= Parent_RelevantChangedControls;
            }

            _prevParent = Parent;

            if (Parent != null)
            {
                Parent.BackColorChanged += Parent_RelevantChanged;
                Parent.BackgroundImageChanged += Parent_RelevantChanged;
                Parent.SizeChanged += Parent_RelevantChanged;
                Parent.ControlAdded += Parent_RelevantChangedControls;
                Parent.ControlRemoved += Parent_RelevantChangedControls;

                // ensure this selector is behind other sibling controls so they remain visible
                try
                {
                    // Put at the back of parent's z-order so siblings appear above it
                    Parent.Controls.SetChildIndex(this, Parent.Controls.Count - 1);
                }
                catch
                {
                    try { SendToBack(); } catch { }
                }
            }

            _bgCacheValid = false;
            _bgCache?.Dispose();
            _bgCache = null;
            Invalidate();
        }


        void Parent_RelevantChanged(object s, EventArgs e) { _bgCacheValid = false; Invalidate(); }
        void Parent_RelevantChangedControls(object s, ControlEventArgs e) { _bgCacheValid = false; Invalidate(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();
                _bgCache?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            UpdateAutoSize();
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            _bgCacheValid = false;
        }

        void AnimationTimer_Tick(object s, EventArgs e)
        {
            _animationProgress += _animationSpeed;
            if (_animationProgress >= 1f)
            {
                _animationProgress = 1f;
                _animationTimer.Stop();
                _selectedIndex = _animationTargetIndex;
                _selectedChildIndex = _animationTargetChildIndex;
            }
            Invalidate();
        }

        bool IsValidSelection(int parentIndex, int childIndex)
        {
            if (_buttons == null || parentIndex < 0 || parentIndex >= _buttons.Count)
                return false;

            if (childIndex < 0)
                return true;

            var parent = _buttons[parentIndex];
            return parent != null && childIndex < parent.Children.Count;
        }

        Rectangle GetButtonBounds(int parentIndex, int childIndex)
        {
            var button = GetButton(parentIndex, childIndex);
            return button == null ? Rectangle.Empty : button.Bounds;
        }

        void OnSelectionChanged()
        {
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            SelectedButtonChanged?.Invoke(this, EventArgs.Empty);
        }

        void StartAnimation(int targetIndex, int targetChildIndex)
        {
            if (!IsValidSelection(targetIndex, targetChildIndex)) return;

            if (targetChildIndex >= 0)
                _buttons[targetIndex].Expanded = true;

            _animationTargetIndex = targetIndex;
            _animationTargetChildIndex = targetChildIndex;

            Rectangle selectedBounds = GetButtonBounds(_selectedIndex, _selectedChildIndex);
            Rectangle targetBounds = GetButtonBounds(targetIndex, targetChildIndex);

            if (selectedBounds.IsEmpty || targetBounds.IsEmpty)
            {
                _selectedIndex = targetIndex;
                _selectedChildIndex = targetChildIndex;
                _animationProgress = 1f;
                _animationTimer.Stop();
                Invalidate();
                return;
            }

            _animationStartBounds = selectedBounds;
            _animationEndBounds = targetBounds;
            _animationProgress = 0f;

            Rectangle inv = Rectangle.Union(_animationStartBounds, _animationEndBounds);
            inv.Inflate(6, 6);
            Invalidate(inv);
            _animationTimer.Start();
        }

        void CaptureParentBackground()
        {
            _bgCache?.Dispose();
            _bgCache = null;
            _bgCacheValid = false;

            if (Parent == null || Width <= 0 || Height <= 0) return;

            try
            {
                // If parent has a background image, draw it scaled into our cache.
                if (Parent.BackgroundImage != null)
                {
                    var bmp = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Parent.BackColor);
                        // Draw the parent's background image scaled to our size
                        g.DrawImage(Parent.BackgroundImage, new Rectangle(0, 0, Width, Height));
                    }
                    _bgCache = bmp;
                    _bgCacheValid = true;
                    return;
                }

                // Otherwise, just fill with parent's BackColor (no hiding siblings).
                var solid = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(solid))
                {
                    g.Clear(Parent.BackColor);
                }
                _bgCache = solid;
                _bgCacheValid = true;
            }
            catch
            {
                try
                {
                    var bmp = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(bmp)) g.Clear(Parent.BackColor);
                    _bgCache = bmp;
                    _bgCacheValid = true;
                }
                catch
                {
                    _bgCacheValid = false;
                    _bgCache?.Dispose();
                    _bgCache = null;
                }
            }
        }


        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || Parent == null)
            {
                e.Graphics.Clear(BackColor);
                return;
            }

            if (!_bgCacheValid || _bgCache == null || _bgCache.Width != Width || _bgCache.Height != Height)
                CaptureParentBackground();

            if (_bgCacheValid && _bgCache != null) e.Graphics.DrawImageUnscaled(_bgCache, 0, 0);
            else e.Graphics.Clear(Parent.BackColor);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // always draw background (even if no buttons)
            base.OnPaintBackground(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            ValidateSelection();
            LayoutButtons();

            DrawUnselectedBackgrounds(g);
            DrawChildConnectors(g);
            DrawSelectedBackground(g);
            DrawHoverBackground(g);
            DrawButtonContents(g);
        }

        void LayoutButtons()
        {
            ClearButtonBounds();

            if (_buttons.Count == 0)
                return;

            if (_orientation == PageSelectorOrientation.Vertical)
                LayoutVerticalButtons();
            else
                LayoutHorizontalButtons();
        }

        void ClearButtonBounds()
        {
            if (_buttons == null)
                return;

            foreach (var button in _buttons)
            {
                if (button == null)
                    continue;

                button.Bounds = Rectangle.Empty;
                foreach (var child in button.Children)
                    if (child != null)
                        child.Bounds = Rectangle.Empty;
            }
        }

        void LayoutHorizontalButtons()
        {
            int x = _padding;
            int y = (Height - _buttonHeight) / 2;

            for (int i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                int buttonWidth = GetButtonWidth(button, IsLayoutSelected(i, -1), false);
                button.Bounds = new Rectangle(x, y, buttonWidth, _buttonHeight);
                x += buttonWidth;

                if (button != null && button.Expanded && button.Children.Count > 0)
                {
                    for (int childIndex = 0; childIndex < button.Children.Count; childIndex++)
                    {
                        x += _buttonSpacing;
                        int childWidth = GetButtonWidth(button.Children[childIndex], IsLayoutSelected(i, childIndex), true);
                        button.Children[childIndex].Bounds = new Rectangle(x, y, childWidth, _buttonHeight);
                        x += childWidth;
                    }
                }

                if (i < _buttons.Count - 1)
                    x += _buttonSpacing;
            }
        }

        void LayoutVerticalButtons()
        {
            int y = _padding;
            int rootWidth = Math.Max(1, Width - (_padding * 2));

            for (int i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                button.Bounds = new Rectangle(_padding, y, rootWidth, _buttonHeight);
                y += _buttonHeight;

                if (button.Expanded && button.Children.Count > 0)
                {
                    y += _childButtonSpacing;
                    int childX = _padding + _childIndent;
                    int childWidth = Math.Max(1, Width - childX - _padding);
                    y = LayoutChildButtons(button, childX, y, childWidth);
                }

                if (i < _buttons.Count - 1)
                    y += _buttonSpacing;
            }
        }

        int LayoutChildButtons(PageButton parent, int x, int y, int width)
        {
            if (parent == null || !parent.Expanded)
                return y;

            for (int i = 0; i < parent.Children.Count; i++)
            {
                parent.Children[i].Bounds = new Rectangle(x, y, width, _childButtonHeight);
                y += _childButtonHeight;
                if (i < parent.Children.Count - 1)
                    y += _childButtonSpacing;
            }

            return y;
        }

        bool HasExpandedChildren()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                if (button != null && button.Expanded && button.Children.Count > 0)
                    return true;
            }

            return false;
        }

        void DrawUnselectedBackgrounds(Graphics g)
        {
            if (_buttons.Count == 0)
                return;

            if (_orientation == PageSelectorOrientation.Horizontal)
            {
                var totalBounds = new Rectangle(_buttons[0].Bounds.X, _buttons[0].Bounds.Y, _buttons[_buttons.Count - 1].Bounds.Right - _buttons[0].Bounds.X, _buttonHeight);
                FillRoundedButton(g, totalBounds, _unselectedColor);

                for (int i = 0; i < _buttons.Count; i++)
                    DrawChildBackgrounds(g, _buttons[i]);

                return;
            }

            for (int i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                FillRoundedButton(g, button.Bounds, _unselectedColor);
                DrawChildBackgrounds(g, button);
            }
        }

        void DrawChildBackgrounds(Graphics g, PageButton parent)
        {
            if (parent == null || !parent.Expanded)
                return;

            for (int i = 0; i < parent.Children.Count; i++)
                FillRoundedButton(g, parent.Children[i].Bounds, _unselectedColor);
        }

        void DrawSelectedBackground(Graphics g)
        {
            if (_animationTimer.Enabled && _animationProgress < 1f)
            {
                float p = _animationProgress;
                var animated = new Rectangle(
                    (int)(_animationStartBounds.X + (_animationEndBounds.X - _animationStartBounds.X) * p),
                    (int)(_animationStartBounds.Y + (_animationEndBounds.Y - _animationStartBounds.Y) * p),
                    (int)(_animationStartBounds.Width + (_animationEndBounds.Width - _animationStartBounds.Width) * p),
                    (int)(_animationStartBounds.Height + (_animationEndBounds.Height - _animationStartBounds.Height) * p));
                FillRoundedButton(g, animated, _selectedColor);
                return;
            }

            FillRoundedButton(g, GetButtonBounds(_selectedIndex, _selectedChildIndex), _selectedColor);
        }

        void DrawHoverBackground(Graphics g)
        {
            if (_hoveredIndex < 0)
                return;

            if (IsSameButton(_hoveredIndex, _hoveredChildIndex, GetLayoutSelectedIndex(), GetLayoutSelectedChildIndex()) ||
                IsSameButton(_hoveredIndex, _hoveredChildIndex, _animationTargetIndex, _animationTargetChildIndex))
                return;

            FillRoundedButton(g, GetButtonBounds(_hoveredIndex, _hoveredChildIndex), _hoverColor);
        }

        void DrawChildConnectors(Graphics g)
        {
            if (_orientation != PageSelectorOrientation.Vertical)
                return;

            Color connectorColor = Color.FromArgb(Math.Min(90, (int)_textColor.A), _textColor);
            using (var pen = new Pen(connectorColor, 1f))
            {
                for (int i = 0; i < _buttons.Count; i++)
                {
                    var parent = _buttons[i];
                    if (parent == null || !parent.Expanded || parent.Children.Count == 0)
                        continue;

                    int lineX = parent.Bounds.X + Math.Max(6, _childIndent / 2);
                    int lastChildCenterY = parent.Children[parent.Children.Count - 1].Bounds.Top + (parent.Children[parent.Children.Count - 1].Bounds.Height / 2);
                    g.DrawLine(pen, lineX, parent.Bounds.Bottom, lineX, lastChildCenterY);

                    for (int childIndex = 0; childIndex < parent.Children.Count; childIndex++)
                    {
                        Rectangle childBounds = parent.Children[childIndex].Bounds;
                        int childCenterY = childBounds.Top + (childBounds.Height / 2);
                        g.DrawLine(pen, lineX, childCenterY, childBounds.X - 4, childCenterY);
                    }
                }
            }
        }

        void DrawButtonContents(Graphics g)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                DrawButtonContent(g, button, IsLayoutSelected(i, -1), false);

                if (button == null || !button.Expanded)
                    continue;

                for (int childIndex = 0; childIndex < button.Children.Count; childIndex++)
                    DrawButtonContent(g, button.Children[childIndex], IsLayoutSelected(i, childIndex), true);
            }
        }

        void DrawButtonContent(Graphics g, PageButton button, bool isSelected, bool isChild)
        {
            if (button == null || button.Bounds.IsEmpty)
                return;

            Rectangle bounds = button.Bounds;
            Color textColor = isSelected ? _selectedTextColor : _textColor;
            string text = button.Text ?? string.Empty;
            Image icon = button.Icon;
            bool showIcon = icon != null && ShouldShowIcon(isSelected);
            bool showArrow = !isChild && button.Children.Count > 0;
            int horizontalPadding = isChild ? _childButtonPadding : _buttonPadding;
            int arrowReserve = showArrow ? _expandArrowSize + _iconSpacing : 0;

            Size textSize = TextRenderer.MeasureText(
                text,
                Font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

            int textWidth = Math.Max(1, textSize.Width);
            int iconWidth = showIcon ? _iconSize + _iconSpacing : 0;
            int availableLeft = bounds.X + horizontalPadding;
            int availableRight = bounds.Right - horizontalPadding - arrowReserve;
            int contentStartX = availableLeft;

            if (_orientation == PageSelectorOrientation.Horizontal)
            {
                int availableWidth = Math.Max(1, availableRight - availableLeft);
                int totalContentWidth = textWidth + iconWidth;
                contentStartX = availableLeft + Math.Max(0, (availableWidth - totalContentWidth) / 2);
            }

            if (showIcon && icon != null)
            {
                float iconY = bounds.Y + (bounds.Height - _iconSize) / 2f;
                var iconRect = new RectangleF(contentStartX, iconY, _iconSize, _iconSize);
                if (TryGetIconTintColor(button, isSelected, out Color iconTintColor))
                    DrawTintedIcon(g, icon, iconRect, iconTintColor);
                else
                    g.DrawImage(icon, iconRect);

                contentStartX += _iconSize + _iconSpacing;
            }

            var textRect = new Rectangle(
                contentStartX,
                bounds.Y,
                Math.Max(1, availableRight - contentStartX),
                bounds.Height);

            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                                    | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;

            TextRenderer.DrawText(g, text, Font, textRect, textColor, flags);

            if (showArrow)
                DrawExpandChevron(g, bounds, textColor, button.Expanded, horizontalPadding);
        }

        void DrawExpandChevron(Graphics g, Rectangle bounds, Color color, bool expanded, int horizontalPadding)
        {
            int size = Math.Max(6, _expandArrowSize);
            float centerX = bounds.Right - horizontalPadding - (size / 2f);
            float centerY = bounds.Top + (bounds.Height / 2f);

            PointF[] points = expanded
                ? new[]
                {
                    new PointF(centerX - (size / 2f), centerY + (size / 4f)),
                    new PointF(centerX, centerY - (size / 4f)),
                    new PointF(centerX + (size / 2f), centerY + (size / 4f))
                }
                : new[]
                {
                    new PointF(centerX - (size / 4f), centerY - (size / 2f)),
                    new PointF(centerX + (size / 4f), centerY),
                    new PointF(centerX - (size / 4f), centerY + (size / 2f))
                };

            using (var pen = new Pen(color, 1.6f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;
                g.DrawLines(pen, points);
            }
        }

        void FillRoundedButton(Graphics g, Rectangle bounds, Color color)
        {
            if (bounds.IsEmpty || color.A == 0)
                return;

            using (var path = CreateButtonPath(bounds, true))
            using (var brush = new SolidBrush(color))
                g.FillPath(brush, path);
        }

        bool IsSameButton(int parentA, int childA, int parentB, int childB)
        {
            return parentA == parentB && childA == childB;
        }

        GraphicsPath CreateButtonPath(Rectangle rect, bool isSelected)
        {
            var path = new GraphicsPath();
            float radius = Math.Min(_buttonRadius, Math.Min(rect.Width, rect.Height) / 2f);
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            float d = radius * 2;
            if (isSelected)
            {
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
            }
            else path.AddRectangle(rect);
            return path;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            TryHitButton(e.Location, out int newHovered, out int newHoveredChild);
            if (newHovered != _hoveredIndex || newHoveredChild != _hoveredChildIndex)
            {
                _hoveredIndex = newHovered;
                _hoveredChildIndex = newHoveredChild;
                Cursor = _hoveredIndex >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hoveredIndex = -1; _hoveredChildIndex = -1; Cursor = Cursors.Default; Invalidate(); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (!TryHitButton(e.Location, out int parentIndex, out int childIndex))
                return;

            var button = GetButton(parentIndex, childIndex);
            if (button == null)
                return;

            if (childIndex < 0)
            {
                SelectButton(parentIndex, -1);
                if (button.Children.Count > 0)
                    button.Expanded = !button.Expanded;
            }
            else
            {
                SelectButton(parentIndex, childIndex);
            }

            if (button.TargetPanel != null && button.TargetPanel.Parent != null)
                button.TargetPanel.BringToFront();
        }

        bool TryHitButton(Point location, out int parentIndex, out int childIndex)
        {
            LayoutButtons();

            parentIndex = -1;
            childIndex = -1;

            for (int i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                if (button == null)
                    continue;

                if (button.Bounds.Contains(location))
                {
                    parentIndex = i;
                    return true;
                }

                if (!button.Expanded)
                    continue;

                for (int child = 0; child < button.Children.Count; child++)
                {
                    if (button.Children[child].Bounds.Contains(location))
                    {
                        parentIndex = i;
                        childIndex = child;
                        return true;
                    }
                }
            }

            return false;
        }

        GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(rect); return path; }
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        bool ShouldShowIcon(bool isSelected)
        {
            switch (_iconVisibility) { case IconVisibility.Always: return true; case IconVisibility.SelectedOnly: return isSelected; case IconVisibility.Never: return false; default: return true; }
        }

        bool TryGetIconTintColor(PageButton button, bool isSelected, out Color tintColor)
        {
            if (isSelected && !_selectedIconColor.IsEmpty)
            {
                tintColor = _selectedIconColor;
                return true;
            }

            if (!isSelected && !_unselectedIconColor.IsEmpty)
            {
                tintColor = _unselectedIconColor;
                return true;
            }

            if (button.TintIcon)
            {
                tintColor = button.IconTintColor;
                return true;
            }

            tintColor = Color.Empty;
            return false;
        }

        void DrawTintedIcon(Graphics g, Image icon, RectangleF rect, Color tintColor)
        {
            if (icon == null)
                return;

            using (var attributes = new ImageAttributes())
            {
                var matrix = new ColorMatrix(new float[][]
                {
                    new float[] { tintColor.R / 255f, 0, 0, 0, 0 },
                    new float[] { 0, tintColor.G / 255f, 0, 0, 0 },
                    new float[] { 0, 0, tintColor.B / 255f, 0, 0 },
                    new float[] { 0, 0, 0, tintColor.A / 255f, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                });

                attributes.SetColorMatrix(matrix);
                g.DrawImage(icon, Rectangle.Round(rect), 0, 0, icon.Width, icon.Height, GraphicsUnit.Pixel, attributes);
            }
        }
    }

    public class PageButtonCollection : Collection<PageButton>
    {
        readonly VortexPageSelector _owner;
        readonly PageButton _ownerButton;

        public PageButtonCollection(VortexPageSelector owner) { _owner = owner; }
        internal PageButtonCollection(PageButton ownerButton) { _ownerButton = ownerButton; }

        protected override void InsertItem(int index, PageButton item)
        {
            base.InsertItem(index, item);
            if (item != null) item.PropertyChanged += OnButtonPropertyChanged;
            OnCollectionChanged();
        }

        protected override void RemoveItem(int index)
        {
            if (index >= 0 && index < Count && this[index] != null)
                this[index].PropertyChanged -= OnButtonPropertyChanged;
            base.RemoveItem(index);
            OnCollectionChanged();
        }

        protected override void SetItem(int index, PageButton item)
        {
            if (index >= 0 && index < Count && this[index] != null)
                this[index].PropertyChanged -= OnButtonPropertyChanged;
            base.SetItem(index, item);
            if (item != null) item.PropertyChanged += OnButtonPropertyChanged;
            OnCollectionChanged();
        }

        protected override void ClearItems()
        {
            foreach (var b in this)
                if (b != null)
                    b.PropertyChanged -= OnButtonPropertyChanged;
            base.ClearItems();
            OnCollectionChanged();
        }

        void OnButtonPropertyChanged(object s, EventArgs e) { OnCollectionChanged(); }

        void OnCollectionChanged()
        {
            _owner?.UpdateAutoSize();
            _owner?.Invalidate();
            _ownerButton?.OnPropertyChanged();
        }
    }

    public class PageButton
    {
        string _text = "Button";
        Image _icon;
        bool _tintIcon;
        bool _expanded;
        Color _iconTintColor = Color.White;
        Control _targetPanel;
        readonly PageButtonCollection _children;

        public PageButton()
        {
            _children = new PageButtonCollection(this);
        }

        [Category("Appearance"), Description("Button text")] public string Text { get => _text; set { _text = value; OnPropertyChanged(); } }
        [Category("Appearance"), Description("Button icon")] public Image Icon { get => _icon; set { _icon = value; OnPropertyChanged(); } }
        [Category("Appearance"), DefaultValue(false), Description("Tint the icon using IconTintColor")] public bool TintIcon { get => _tintIcon; set { _tintIcon = value; OnPropertyChanged(); } }
        [Category("Appearance"), Description("Icon tint color when TintIcon is enabled")] public Color IconTintColor { get => _iconTintColor; set { _iconTintColor = value; OnPropertyChanged(); } }
        [Category("Behavior"), DefaultValue(false), Description("Show this button's child buttons")] public bool Expanded { get => _expanded; set { _expanded = value; OnPropertyChanged(); } }
        [Category("Behavior"), Description("Target panel")] public Control TargetPanel { get => _targetPanel; set { _targetPanel = value; OnPropertyChanged(); } }
        [Category("Child Buttons"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Description("Child buttons displayed under this button when Expanded is true")] public PageButtonCollection Children => _children;
        [Browsable(false)] public Rectangle Bounds { get; set; }
        public event EventHandler PropertyChanged;
        internal void OnPropertyChanged() { PropertyChanged?.Invoke(this, EventArgs.Empty); }
    }
}
