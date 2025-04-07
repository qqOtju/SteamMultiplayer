using UnityEngine;

namespace Tools.MyGridLayout
{
    [ExecuteInEditMode]
    public abstract class AbstractGridLayout : MonoBehaviour
    {
        protected float CellHeight;
        protected float CellWidth;
        protected int ChildCount;

        protected void Align(int rows, int columns, Vector2 spacing, Vector2 paddingVertical, Vector2 paddingHorizontal, bool fromBottomToTop = false)
        {
            ChildCount = transform.childCount;
            if(ChildCount == 0) return;
            var exit = false;
            CellWidth = 1 / (float)columns - spacing.x + spacing.x/columns - paddingHorizontal.x / columns - paddingHorizontal.y/columns;
            CellHeight = 1 / (float)rows - spacing.y + spacing.y / rows - paddingVertical.x / rows - paddingVertical.y / rows;
            
            if(fromBottomToTop)
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        if(i * columns + j >= ChildCount)
                        {
                            exit = true;
                            break;
                        }
                    
                        var childRect = transform.GetChild(i * columns + j).GetComponent<RectTransform>();
                        childRect.anchorMax = new Vector2(CellWidth * (j + 1) + spacing.x * j + paddingHorizontal.x,  CellHeight * i + CellHeight + spacing.y * i  + paddingVertical.x);
                        childRect.anchorMin = new Vector2(CellWidth * j + spacing.x * j + paddingHorizontal.x, CellHeight * i + spacing.y * i  + paddingVertical.x);
                        childRect.offsetMax = Vector2.zero;
                        childRect.offsetMin = Vector2.zero;
                    }
                    if (exit)
                        break;
                }
            else
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        if(i * columns + j >= ChildCount)
                        {
                            exit = true;
                            break;
                        }
                    
                        var childRect = transform.GetChild(i * columns + j).GetComponent<RectTransform>();
                        childRect.anchorMax = new Vector2(CellWidth * (j + 1) + spacing.x * j + paddingHorizontal.x,  1 - (CellHeight * i + spacing.y * i  + paddingVertical.y));
                        childRect.anchorMin = new Vector2(CellWidth * j + spacing.x * j + paddingHorizontal.x, 1 - (CellHeight * (i + 1) + spacing.y * i  + paddingVertical.y));
                        childRect.offsetMax = Vector2.zero;
                        childRect.offsetMin = Vector2.zero;
                    }
                    if (exit)
                        break;
                }
        }

        public abstract void Align();
    }
}