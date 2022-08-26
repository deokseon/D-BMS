public class JudgeManager
{
    private static JudgeManager inst = null;

    public static JudgeManager instance
    {
        get
        {
            if (inst == null) inst = new JudgeManager();
            return inst;
        }
        private set { inst = value; }
    }

    public JudgeType Judge(Note n, double currentTime)
    {
        double diff = Utility.Dabs(n.timing - currentTime) * 1000;

        if (diff <= 22.0d) { return JudgeType.KOOL; }
        else if (diff <= 55.0d) { return JudgeType.COOL; }
        else if (diff <= 115.0d) { return JudgeType.GOOD; }
        else if (diff <= 175.0d) { return JudgeType.MISS; }
        else if (currentTime > n.timing) { return JudgeType.FAIL; }
        else { return JudgeType.IGNORE; }
    }
}

public enum JudgeType
{
    IGNORE,
    FAIL,
    MISS,
    GOOD,
    COOL,
    KOOL
}
