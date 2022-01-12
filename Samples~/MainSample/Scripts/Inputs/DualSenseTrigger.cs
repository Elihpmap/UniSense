using UniSense;

namespace DualSenseSample.Inputs
{
    /// <summary>
    /// Component to set the DualSense Triggers.
    /// </summary>
    public class DualSenseTrigger : AbstractDualSenseBehaviour
    {
        #region Properties for Left Trigger
        public int LeftTriggerEffectType
        {
            get => (int)leftTriggerState.EffectType;
            set => leftTriggerState.EffectType = SetTriggerEffectType(value);
        }

        #region Continuous Resistance Properties
        public float LeftContinuousForce
        {
            get => leftTriggerState.ContinuousResistance.Force;
            set => leftTriggerState.ContinuousResistance.Force = (byte)(value * 255);
        }

        public float LeftContinuousStartPosition
        {
            get => leftTriggerState.ContinuousResistance.StartPosition;
            set => leftTriggerState.ContinuousResistance.StartPosition = (byte)(value * 255);
        }
        #endregion

        #region Section Resistance Properties
        public float LeftSectionForce
        {
            get => leftTriggerState.SectionResistance.Force;
            set => leftTriggerState.SectionResistance.Force = (byte)(value * 255);
        }

        public float LeftSectionStartPosition
        {
            get => leftTriggerState.SectionResistance.StartPosition;
            set => leftTriggerState.SectionResistance.StartPosition = (byte)(value * 255);
        }

        public float LeftSectionEndPosition
        {
            get => leftTriggerState.SectionResistance.EndPosition;
            set => leftTriggerState.SectionResistance.EndPosition = (byte)(value * 255);
        }
        #endregion

        #region EffectEx Properties
        public float LeftEffectStartPosition
        {
            get => leftTriggerState.EffectEx.StartPosition;
            set => leftTriggerState.EffectEx.StartPosition = (byte)(value * 255);
        }

        public float LeftEffectBeginForce
        {
            get => leftTriggerState.EffectEx.BeginForce;
            set => leftTriggerState.EffectEx.BeginForce = (byte)(value * 255);
        }

        public float LeftEffectMiddleForce
        {
            get => leftTriggerState.EffectEx.MiddleForce;
            set => leftTriggerState.EffectEx.MiddleForce = (byte)(value * 255);
        }

        public float LeftEffectEndForce
        {
            get => leftTriggerState.EffectEx.EndForce;
            set => leftTriggerState.EffectEx.EndForce = (byte)(value * 255);
        }

        public float LeftEffectFrequency
        {
            get => leftTriggerState.EffectEx.Frequency;
            set => leftTriggerState.EffectEx.Frequency = (byte)(value * 255);
        }

        public bool LeftEffectKeepEffect
        {
            get => leftTriggerState.EffectEx.KeepEffect;
            set => leftTriggerState.EffectEx.KeepEffect = value;
        }
        #endregion
        #endregion

        #region Properties for Right Trigger
        public int RightTriggerEffectType
        {
            get => (int)rightTriggerState.EffectType;
            set => rightTriggerState.EffectType = SetTriggerEffectType(value);
        }

        #region Continuous Resistance Properties
        public float RightContinuousForce
        {
            get => rightTriggerState.ContinuousResistance.Force;
            set => rightTriggerState.ContinuousResistance.Force = (byte)(value * 255);
        }

        public float RightContinuousStartPosition
        {
            get => rightTriggerState.ContinuousResistance.StartPosition;
            set => rightTriggerState.ContinuousResistance.StartPosition = (byte)(value * 255);
        }
        #endregion

        #region Section Resistance Properties
        public float RightSectionForce
        {
            get => rightTriggerState.SectionResistance.Force;
            set => rightTriggerState.SectionResistance.Force = (byte)(value * 255);
        }

        public float RightSectionStartPosition
        {
            get => rightTriggerState.SectionResistance.StartPosition;
            set => rightTriggerState.SectionResistance.StartPosition = (byte)(value * 255);
        }

        public float RightSectionEndPosition
        {
            get => rightTriggerState.SectionResistance.EndPosition;
            set => rightTriggerState.SectionResistance.EndPosition = (byte)(value * 255);
        }
        #endregion

        #region EffectEx Properties
        public float RightEffectStartPosition
        {
            get => rightTriggerState.EffectEx.StartPosition;
            set => rightTriggerState.EffectEx.StartPosition = (byte)(value * 255);
        }

        public float RightEffectBeginForce
        {
            get => rightTriggerState.EffectEx.BeginForce;
            set => rightTriggerState.EffectEx.BeginForce = (byte)(value * 255);
        }

        public float RightEffectMiddleForce
        {
            get => rightTriggerState.EffectEx.MiddleForce;
            set => rightTriggerState.EffectEx.MiddleForce = (byte)(value * 255);
        }

        public float RightEffectEndForce
        {
            get => rightTriggerState.EffectEx.EndForce;
            set => rightTriggerState.EffectEx.EndForce = (byte)(value * 255);
        }

        public float RightEffectFrequency
        {
            get => rightTriggerState.EffectEx.Frequency;
            set => rightTriggerState.EffectEx.Frequency = (byte)(value * 255);
        }

        public bool RightEffectKeepEffect
        {
            get => rightTriggerState.EffectEx.KeepEffect;
            set => rightTriggerState.EffectEx.KeepEffect = value;
        }
        #endregion
        #endregion

        private DualSenseTriggerState leftTriggerState;
        private DualSenseTriggerState rightTriggerState;

        private void Awake()
        {
            leftTriggerState = new DualSenseTriggerState
            {
                EffectType = DualSenseTriggerEffectType.ContinuousResistance,
                EffectEx = new DualSenseEffectExProperties(),
                SectionResistance = new DualSenseSectionResistanceProperties(),
                ContinuousResistance = new DualSenseContinuousResistanceProperties()
            };

            rightTriggerState = new DualSenseTriggerState
            {
                EffectType = DualSenseTriggerEffectType.ContinuousResistance,
                EffectEx = new DualSenseEffectExProperties(),
                SectionResistance = new DualSenseSectionResistanceProperties(),
                ContinuousResistance = new DualSenseContinuousResistanceProperties()
            };
        }

        private void Update()
        {
            DualSense?.SetTriggerState(leftTriggerState, rightTriggerState, false);
        }

        private DualSenseTriggerEffectType SetTriggerEffectType(int index)
        {
            if (index == 0) return DualSenseTriggerEffectType.ContinuousResistance;
            if (index == 1) return DualSenseTriggerEffectType.SectionResistance;
            if (index == 2) return DualSenseTriggerEffectType.EffectEx;

            return DualSenseTriggerEffectType.NoResistance;
        }
    }
}
