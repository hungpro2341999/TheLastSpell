using TPLib.Log;

public class CLoggerTest : CLogger<CLoggerTest>
{
	public void Start()
	{
		Test();
	}

	public void Test()
	{
		Log("TEST LOG 1");
		Log("TEST LOG 2", CLogLevel.DETAILED, forcePrintInUnity: true);
		Log("TEST LOG 3", CLogLevel.NORMAL, forcePrintInUnity: true);
		Log("TEST LOG 4", CLogLevel.MAJOR, forcePrintInUnity: true);
		LogFormat("{0} {1} {2} {3}", CLogLevel.MAJOR, true, false, "TEST", "LOG", "FORMAT", "5");
		LogFormat("{0} {1} {2} {3}", this, CLogLevel.MAJOR, true, false, "TEST", "LOG", "FORMAT", "6");
		Log("TEST LOG 7", this, CLogLevel.MAJOR, forcePrintInUnity: true);
		LogWarning("TEST LOGWARNING 1");
		LogWarning("TEST LOGWARNING 2", CLogLevel.DETAILED);
		LogWarning("TEST LOGWARNING 3");
		LogWarning("TEST LOGWARNING 4", CLogLevel.MAJOR);
		LogWarningFormat("{0} {1} {2} {3}", CLogLevel.MAJOR, true, false, "TEST", "LOGWARNING", "FORMAT", "5");
		LogWarningFormat("{0} {1} {2} {3}", this, CLogLevel.MAJOR, true, false, "TEST", "LOGWARNING", "FORMAT", "6");
		LogWarning("TEST LOGWARNING 7", this, CLogLevel.MAJOR);
		LogError("TEST LOGERROR 1");
		LogError("TEST LOGERROR 2", CLogLevel.DETAILED);
		LogError("TEST LOGERROR 3");
		LogError("TEST LOGERROR 4", CLogLevel.MAJOR);
		LogErrorFormat("{0} {1} {2} {3}", CLogLevel.MAJOR, true, true, "TEST", "LOGERROR", "FORMAT", "5");
		LogErrorFormat("{0} {1} {2} {3}", this, CLogLevel.MAJOR, true, true, "TEST", "LOGERROR", "FORMAT", "6");
		LogError("TEST LOGERROR 7", this, CLogLevel.MAJOR);
	}
}
