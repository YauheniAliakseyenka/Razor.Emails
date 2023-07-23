using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;

namespace Razor.Emails.Core
{
	public partial class HtmlRenderer
    {
        private static readonly HtmlEncoder htmlEncoder = HtmlEncoder.Default;
        private static readonly HashSet<string> SelfClosingElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr"
        };

        internal void WriteComponentHtml(int componentId, TextWriter output)
		{
			Dispatcher.AssertAccess();

			var frames = GetCurrentRenderTreeFrames(componentId);

			RenderFrames(componentId, output, frames, position:0, frames.Count);
		}

        private int RenderFrames(int componentId, TextWriter output, ArrayRange<RenderTreeFrame> frames, int position, int maxElements)
        {
            var nextPosition = position;
            var endPosition = position + maxElements;

            while (position < endPosition)
            {
                nextPosition = RenderCore(componentId, output, frames, position);

                if (position == nextPosition)
                {
                    throw new InvalidOperationException("Could not consume any input");
                }

                position = nextPosition;
            }

            return nextPosition;
        }

        private int RenderCore(int componentId, TextWriter output, ArrayRange<RenderTreeFrame> frames, int position)
        {
            ref var frame = ref frames.Array[position];

            switch (frame.FrameType)
            {
                case RenderTreeFrameType.Element:
                    position = RenderElement(componentId, output, frames, position);
                    break;

                case RenderTreeFrameType.Text:
                    htmlEncoder.Encode(output, frame.TextContent);
                    position++;
                    break;

                case RenderTreeFrameType.Markup:
                    output.Write(frame.MarkupContent);
                    position++;
                    break;

                case RenderTreeFrameType.Component:
                    position = RenderChildComponent(output, frames, position);
                    break;

                case RenderTreeFrameType.Region:
                    position = RenderFrames(componentId, output, frames, position + 1, frame.RegionSubtreeLength - 1);
                    break;

                case RenderTreeFrameType.ElementReferenceCapture:
                case RenderTreeFrameType.ComponentReferenceCapture:
                    position++;
                    break;

                case RenderTreeFrameType.Attribute:
                    throw new InvalidOperationException($"Attributes should only be encountered within {nameof(RenderElement)}");

                default:
                    throw new InvalidOperationException($"Invalid element frame type '{frame.FrameType}'");
            }

            return position;
        }

        private int RenderElement(int componentId, TextWriter output, ArrayRange<RenderTreeFrame> frames, int position)
        {
            ref var frame = ref frames.Array[position];

            ValidateAndThrow(frame);

            output.Write('<');
            output.Write(frame.ElementName);

            int afterElement;
            var afterAttributes = RenderAttributes(output, frames, position + 1, frame.ElementSubtreeLength - 1, includeValueAttribute: false);
            var remainingElements = frame.ElementSubtreeLength + position - afterAttributes;

            if (remainingElements > 0)
            {
                output.Write('>');

                afterElement = RenderChildren(componentId, output, frames, afterAttributes, remainingElements);

                output.Write("</");
                output.Write(frame.ElementName);
                output.Write('>');

                Debug.Assert(afterElement == position + frame.ElementSubtreeLength);

                return afterElement;
            }
            else
            {
                if (SelfClosingElements.Contains(frame.ElementName))
                {
                    output.Write(" />");
                }
                else
                {
                    output.Write("></");
                    output.Write(frame.ElementName);
                    output.Write('>');
                }

                Debug.Assert(afterAttributes == position + frame.ElementSubtreeLength);

                return afterAttributes;
            }
        }

        private static int RenderAttributes(TextWriter output, ArrayRange<RenderTreeFrame> frames, int position, int maxElements, bool includeValueAttribute)
        {
            if (maxElements == 0)
            {
                return position;
            }

            for (var i = 0; i < maxElements; i++)
            {
                var candidateIndex = position + i;
                ref var frame = ref frames.Array[candidateIndex];

                if (frame.FrameType != RenderTreeFrameType.Attribute)
                {
                    if (frame.FrameType == RenderTreeFrameType.ElementReferenceCapture)
                    {
                        continue;
                    }

                    return candidateIndex;
                }

                if (frame.AttributeName.Equals("value", StringComparison.OrdinalIgnoreCase))
                {
                    if (!includeValueAttribute)
                    {
                        continue;
                    }
                }

                switch (frame.AttributeValue)
                {
                    case bool flag when flag:
                        output.Write(' ');
                        output.Write(frame.AttributeName);
                        break;

                    case string value:
                        output.Write(' ');
                        output.Write(frame.AttributeName);
                        output.Write('=');
                        output.Write('\"');
                        htmlEncoder.Encode(output, value);
                        output.Write('\"');
                        break;
                }
            }

            return position + maxElements;
        }

        private int RenderChildren(int componentId, TextWriter output, ArrayRange<RenderTreeFrame> frames, int position, int maxElements)
        {
            if (maxElements == 0)
            {
                return position;
            }

            return RenderFrames(componentId, output, frames, position, maxElements);
        }

        private void RenderChildComponent(TextWriter output, ref RenderTreeFrame componentFrame)
        {
            WriteComponentHtml(componentFrame.ComponentId, output);
        }

        private int RenderChildComponent(TextWriter output, ArrayRange<RenderTreeFrame> frames, int position)
        {
            ref var frame = ref frames.Array[position];

            RenderChildComponent(output, ref frame);

            return position + frame.ComponentSubtreeLength;
        }

        private static void ValidateAndThrow(RenderTreeFrame frame)
        {
            if (string.Equals(frame.ElementName, "textarea", StringComparison.OrdinalIgnoreCase)
                || string.Equals(frame.ElementName, "input", StringComparison.OrdinalIgnoreCase)
                || string.Equals(frame.ElementName, "select", StringComparison.OrdinalIgnoreCase))
			{
                throw new InvalidDataException($"{frame.ElementName} is not supported for emails");
			}
		}
    }
}
