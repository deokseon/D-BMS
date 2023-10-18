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

    public JudgeType Judge(double diff)
    {
        if (diff > 1750000.0d) { return JudgeType.FAIL; }

        switch (diff > 0 ? diff : -diff)
        {
            case double k when (k <= 220000.0d): return JudgeType.KOOL;
            case double k when (k <= 550000.0d): return JudgeType.COOL;
            case double k when (k <= 1100000.0d): return JudgeType.GOOD;
            case double k when (k <= 1750000.0d): return JudgeType.MISS;
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
