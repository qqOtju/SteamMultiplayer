using UnityEngine;

namespace Tools.MyGridLayout
{
    public class Layout: AbstractGridLayout
    {
        [Header("Settings")]
        [SerializeField] private LayoutType _layoutType = LayoutType.Grid;
        [SerializeField] private bool _isAutoAlign = true;
        [SerializeField] private bool _fromBottomToTop;
        [Header("Values")]
        [Min(1)] [SerializeField] private int _columnCount = 1;
        [Min(1)] [SerializeField] private int _rowCount = 1;
        [Range(0,500)] [SerializeField] private float _spacingHorizontal;
        [Range(0,1000)] [SerializeField] private float _spacingVertical;
        [Range(0,1000)] [SerializeField] private float _leftPadding;
        [Range(0,1000)] [SerializeField] private float _rightPadding;
        [Range(0,1000)] [SerializeField] private float _topPadding;
        [Range(0,1000)] [SerializeField] private float _bottomPadding;
        
        private Vector2 Spacing => new Vector2(_spacingHorizontal, _spacingVertical) / 1000;
        private Vector2 HorizontalPadding => new Vector2(_leftPadding, _rightPadding) / 1000;
        private Vector2 VerticalPadding => new Vector2(_topPadding, _bottomPadding) / 1000;
        
        public int ColumnCount
        {
            get => _columnCount;
            set => _columnCount = value;
        }
        
        public int RowCount
        {
            get => _rowCount;
            set => _rowCount = value;
        }
        
        private void Update()
        {
            if(!Application.isPlaying && _isAutoAlign)
                Align();
        }

        public override void Align()
        {
            switch (_layoutType)
            {
                case LayoutType.Horizontal:
                    Align(1, _columnCount, Spacing, VerticalPadding, HorizontalPadding, _fromBottomToTop);
                    break;
                case LayoutType.Vertical:
                    Align(_rowCount, 1, Spacing, VerticalPadding, HorizontalPadding, _fromBottomToTop);
                    break;
                case LayoutType.Grid:
                    Align(_rowCount, _columnCount, Spacing, VerticalPadding, HorizontalPadding, _fromBottomToTop);
                    break;
            }
        }
    }
    
    public enum LayoutType
    {
        Horizontal = 0,
        Vertical = 1,
        Grid = 2
    }
}