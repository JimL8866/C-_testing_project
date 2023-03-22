using CTGAUAardQuadTest.DataClasses.BGRAardvarkPORegression;

namespace CTGAUAardQuadTest.DataClasses
{
    /// <summary>
    /// Can use this class to share data between step definition and model class files.
    /// </summary>
    public class ScriptContext
    {
        public DataSubmittedClass DataSubmit { get; set; } = new DataSubmittedClass();

        public OutSamplesClass OutSamples { get; set; } = new OutSamplesClass();

        public BaseLineSamplesClass BaseLineSamples { get; set; } = new BaseLineSamplesClass();

        public string samplesFolder { get; set; }
    }
}
