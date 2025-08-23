// using System;
// using System.IO;
// using System.Linq;
// using DocumentFormat.OpenXml;
// using DocumentFormat.OpenXml.Packaging;
// using WP = DocumentFormat.OpenXml.Wordprocessing;
// using A = DocumentFormat.OpenXml.Drawing;
// using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
// using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

// namespace CoreAdminWeb.Helpers
// {
// 	/// <summary>
// 	/// Utilities for manipulating DOCX using OpenXML SDK
// 	/// </summary>
// 	public static class DocxHelper
// 	{
// 		/// <summary>
// 		/// Replace all occurrences of a placeholder with text. Handles split runs.
// 		/// </summary>
// 		public static void ReplaceText(WordprocessingDocument doc, string searchValue, string newValue)
// 		{
// 			if (doc == null) throw new ArgumentNullException(nameof(doc));
// 			if (string.IsNullOrEmpty(searchValue)) return;

// 			var main = doc.MainDocumentPart ?? throw new InvalidOperationException("MainDocumentPart is null");

// 			// Simple in-place replacements where placeholder is within a single WP.Text
// 			var texts = main.Document.Descendants<WP.Text>().ToList();
// 			foreach (var t in texts)
// 			{
// 				if (t.Text?.Contains(searchValue) == true)
// 				{
// 					var before = t.Text;
// 					t.Text = before.Replace(searchValue, newValue ?? string.Empty);
// 				}
// 			}

// 			// Handle placeholders broken across multiple WP.Text within the same run/paragraph
// 			var paragraphs = main.Document.Descendants<WP.Paragraph>().ToList();
// 			foreach (var p in paragraphs)
// 			{
// 				var pTexts = p.Descendants<WP.Text>().ToList();
// 				if (pTexts.Count <= 1) continue;

// 				var combined = string.Concat(pTexts.Select(x => x.Text));
// 				if (string.IsNullOrEmpty(combined) || !combined.Contains(searchValue)) continue;

// 				var replaced = combined.Replace(searchValue, newValue ?? string.Empty);

// 				// Remove all existing texts and create a single text node to preserve simplicity
// 				foreach (var tx in pTexts) tx.Remove();
// 				var run = p.GetFirstChild<WP.Run>() ?? p.AppendChild(new WP.Run());
// 				run.AppendChild(new WP.Text(replaced));
// 			}
// 		}

// 		/// <summary>
// 		/// Replace a placeholder with an inline image (PNG/JPG). If placeholder not found, no-op.
// 		/// </summary>
// 		public static void ReplaceTextWithImage(WordprocessingDocument doc, string searchValue, string imagePath, int widthPx = 60, int heightPx = 60)
// 		{
// 			if (doc == null) throw new ArgumentNullException(nameof(doc));
// 			if (string.IsNullOrEmpty(searchValue)) return;
// 			if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath)) return;

// 			var main = doc.MainDocumentPart ?? throw new InvalidOperationException("MainDocumentPart is null");

// 			// Find the first text node containing the placeholder
// 			var textNode = main.Document.Descendants<WP.Text>().FirstOrDefault(t => t.Text?.Contains(searchValue) == true);
// 			if (textNode == null) return; // nothing to do

// 			// Replace text by removing the text node and inserting image in its parent paragraph
// 			var paragraph = textNode.Ancestors<WP.Paragraph>().FirstOrDefault();
// 			if (paragraph == null)
// 			{
// 				// fallback: remove text only
// 				textNode.Text = textNode.Text.Replace(searchValue, string.Empty);
// 				return;
// 			}

// 			// Remove the placeholder text from its run
// 			textNode.Text = textNode.Text.Replace(searchValue, string.Empty);
// 			if (string.IsNullOrEmpty(textNode.Text))
// 			{
// 				var runToClean = textNode.Parent as WP.Run;
// 				textNode.Remove();
// 				// remove empty run
// 				if (runToClean != null && !runToClean.Descendants<WP.Text>().Any()) runToClean.Remove();
// 			}

// 			// Add image part
// 			var imagePartType = InferImagePartType(imagePath);
// 			var imagePart = main.AddImagePart(imagePartType);
// 			using (var fs = File.OpenRead(imagePath))
// 			{
// 				imagePart.FeedData(fs);
// 			}
// 			var relId = main.GetIdOfPart(imagePart);

// 			// Convert pixels to EMU
// 			long cx = PxToEmu(widthPx);
// 			long cy = PxToEmu(heightPx);

// 			// Build drawing structure (Inline)
// 			var element = BuildInlineDrawing(relId, cx, cy);

// 			// Append into a new run within the paragraph
// 			var targetRun = paragraph.AppendChild(new WP.Run());
// 			targetRun.AppendChild(element);
// 		}

// 		// Build Inline drawing (A/DW/PIC)
// 		private static DW.Drawing BuildInlineDrawing(string relId, long cx, long cy)
// 		{
// 			var inline = new DW.Inline(
// 				new DW.Extent() { Cx = cx, Cy = cy },
// 				new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
// 				new DW.DocProperties() { Id = (UInt32Value)1U, Name = "Picture" },
// 				new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks() { NoChangeAspect = true })
// 			);

// 			var graphic = new A.Graphic(
// 				new A.GraphicData(
// 					new PIC.Picture(
// 						new PIC.NonVisualPictureProperties(
// 							new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = "Image" },
// 							new PIC.NonVisualPictureDrawingProperties()
// 						),
// 						new PIC.BlipFill(
// 							new A.Blip() { Embed = relId },
// 							new A.Stretch(new A.FillRectangle())
// 						),
// 						new PIC.ShapeProperties(
// 							new A.Transform2D(
// 								new A.Offset() { X = 0L, Y = 0L },
// 								new A.Extents() { Cx = cx, Cy = cy }
// 							),
// 							new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }
// 						)
// 					)
// 				) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
// 			);

// 			inline.AppendChild(graphic);
// 			return new DW.Drawing(inline);
// 		}

// 		private static ImagePartType InferImagePartType(string path)
// 		{
// 			var ext = Path.GetExtension(path)?.ToLowerInvariant();
// 			return ext switch
// 			{
// 				".png" => ImagePartType.Png,
// 				".jpg" => ImagePartType.Jpeg,
// 				".jpeg" => ImagePartType.Jpeg,
// 				".gif" => ImagePartType.Gif,
// 				_ => ImagePartType.Png
// 			};
// 		}

// 		private static long PxToEmu(int px) => (long)(px * 9525L);

// 		/// <summary>
// 		/// Minimal document debug to help troubleshooting
// 		/// </summary>
// 		public static void DebugDocumentStructure(WordprocessingDocument doc)
// 		{
// 			var main = doc.MainDocumentPart;
// 			if (main == null) { Console.WriteLine("[Debug] MainDocumentPart is null"); return; }
// 			Console.WriteLine($"[Debug] Paragraphs: {main.Document.Descendants<WP.Paragraph>().Count()} | Runs: {main.Document.Descendants<WP.Run>().Count()} | Texts: {main.Document.Descendants<WP.Text>().Count()}");
// 		}
// 	}
// }
