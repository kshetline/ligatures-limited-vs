using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LigaturesLimited
{
  [Export(typeof(IWpfTextViewCreationListener))]
  [Name("LigaturesLimited.TextViews")]
  [ContentType("text")]
  [TextViewRole(PredefinedTextViewRoles.Document)]
  public class DeligaturizerListener : IWpfTextViewCreationListener
  {
    [Import]
    private readonly IClassificationFormatMapService formatService = null;

    private static Dictionary<IWpfTextView, IClassificationFormatMap> formatMaps = new Dictionary<IWpfTextView, IClassificationFormatMap>();

    public void TextViewCreated(IWpfTextView view)
    {
      formatMaps[view] = this.formatService.GetClassificationFormatMap(view);
    }

    public static IClassificationFormatMap GetMap(IWpfTextView view)
    {
      return formatMaps[view];
    }
  }

  internal sealed class Deligaturizer
  {
    private readonly IAdornmentLayer layer;
    private readonly IWpfTextView view;
    private readonly Brush brush = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0x00, 0xff));
    private readonly Pen pen = new Pen(new SolidColorBrush(Colors.Red), 0.5);

    public Deligaturizer(IWpfTextView view)
    {
      if (view == null)
        throw new ArgumentNullException("view");

      layer = view.GetAdornmentLayer("Deligaturizer");
      this.view = view;
      this.view.LayoutChanged += this.OnLayoutChanged;
      this.brush.Freeze();
      this.pen.Freeze();
    }

    internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
    {
      foreach (ITextViewLine line in e.NewOrReformattedLines)
        this.CreateVisuals(line);
    }

    private void CreateVisuals(ITextViewLine line)
    {
      var textViewLines = this.view.TextViewLines;

      for (int charIndex = line.Start; charIndex < line.End; charIndex++) {
        if (this.view.TextSnapshot[charIndex] == '=') {
          var span = new SnapshotSpan(this.view.TextSnapshot, Span.FromBounds(charIndex, charIndex + 1));
          var geometry = textViewLines.GetMarkerGeometry(span);

          if (geometry != null) {
            var props = view.FormattedLineSource.DefaultTextProperties;
            props.Typeface.TryGetGlyphTypeface(out var glyphTypeface);
            var fontSize = props.Typeface.XHeight;
            var glyphIndex = glyphTypeface.CharacterToGlyphMap['='];
            var width = glyphTypeface.AdvanceWidths[glyphIndex] * fontSize;
#pragma warning disable CS0618 // Type or member is obsolete
            var glyphRun = new GlyphRun(glyphTypeface, 0, false, props.FontHintingEmSize, new ushort[] { glyphIndex },
              new Point(0, props.FontHintingEmSize * 0.66), new double[] { width }, null, null, null, null, null, null);
#pragma warning restore CS0618 // Type or member is obsolete
            var drawing = new GlyphRunDrawing(this.brush, glyphRun);
            drawing.Freeze();

            var drawingImage = new DrawingImage(drawing);
            drawingImage.Freeze();

            var image = new Image { Source = drawingImage };

            // Align the image with the top of the bounds of the text geometry
            Canvas.SetLeft(image, geometry.Bounds.Left);
            Canvas.SetTop(image, geometry.Bounds.Top);

            this.layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
          }
        }
      }
    }
  }
}
