using System.Drawing;
using System.Windows.Forms;

namespace UAssetGUI
{
    public class ColorfulTreeView : TreeView
    {
        public ColorfulTreeView() : base()
        {
            this.DrawMode = TreeViewDrawMode.OwnerDrawText;
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            Font font = e.Node.NodeFont ?? e.Node.TreeView.Font;
            Color fore = e.Node.ForeColor;
            if (fore == Color.Empty) fore = e.Node.TreeView.ForeColor;
            if (e.Node == e.Node.TreeView.SelectedNode)
            {
                fore = UAGPalette.HighlightForeColor;
                e.Graphics.FillRectangle(new SolidBrush(UAGPalette.HighlightBackColor), e.Bounds);
                ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, fore, UAGPalette.HighlightBackColor);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, fore, UAGPalette.HighlightBackColor, TextFormatFlags.GlyphOverhangPadding);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(UAGPalette.BackColor), e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, fore, TextFormatFlags.GlyphOverhangPadding);
            }
        }
    }
}
