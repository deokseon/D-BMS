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
        double diff = (n.timing - currentTime) * 1000.0d;
        diff = (diff > 0 ? diff : -diff);

        if (diff > 175.0d && currentTime > n.timing) { return JudgeType.FAIL; }
        switch (diff)
        {
            case double k when (k <= 22.0d): return JudgeType.KOOL;
            case double k when (k <= 55.0d): return JudgeType.COOL;
            case double k when (k <= 115.0d): return JudgeType.GOOD;
            case double k when (k <= 175.0d): return JudgeType.MISS;
            default: return JudgeType.IGNORE;
        }
    }

    public JudgeType Judge(double diff)
    {
        double absDiff = (diff > 0 ? diff : -diff);

        if (diff > 175.0d) { return JudgeType.FAIL; }
        switch (absDiff)
        {
            case double k when (k <= 22.0d): return JudgeType.KOOL;
            case double k when (k <= 55.0d): return JudgeType.COOL;
            case double k when (k <= 115.0d): return JudgeType.GOOD;
            case double k when (k <= 175.0d): return JudgeType.MISS;
            default: return JudgeType.IGNORE;
        }
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
