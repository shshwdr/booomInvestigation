// // ©2015 - 2025 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System.Collections.Generic;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Utils
{
    public static class RectTransformUtils
    {
        public static void SetAnchors(this RectTransform This, Vector2 AnchorMin, Vector2 AnchorMax)
        {
            var OriginalPosition = This.localPosition;
            var OriginalSize = This.sizeDelta;
            var offsetMin = This.offsetMin;
            var offsetMax = This.offsetMax;

            This.anchorMin = AnchorMin;
            This.anchorMax = AnchorMax;

            This.offsetMin = offsetMin;
            This.offsetMax = offsetMax;
            This.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, OriginalSize.x);
            This.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, OriginalSize.y);
            This.localPosition = OriginalPosition;
        }

        
    }
}