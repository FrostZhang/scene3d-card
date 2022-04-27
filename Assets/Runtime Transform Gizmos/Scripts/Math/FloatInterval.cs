using UnityEngine;

namespace RTEditor
{
    public struct FloatInterval
    {
        #region Private Variables
        private float _min;
        private float _max;
        #endregion

        #region Public Properties
        public float Min { get { return _min; } set { if (value < _max) _min = value; } }
        public float Max { get { return _max; } set { if (value > _min) _max = value; } }
        #endregion

        #region Constructors
        public FloatInterval(float min, float max)
        {
            _min = min;
            _max = max;

            if(_min > _max)
            {
                float temp = _min;
                _min = _max;
                _max = temp;
            }
        }
        #endregion

        #region Public Methods
        public bool Contains(float value)
        {
            return _min <= value && value <= _max;
        }

        public float Clamp(float value)
        {
            if (Contains(value)) return value;

            if (value < Min) return Min;
            return Max;
        }
        #endregion
    }
}
