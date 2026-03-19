public class TestHelper
{
    static string? sourceCodeActionV1;
    static string? sourceCodeActionV2;
    static string? sourceCodeActionV3;
    static string? sourceCodeVaccineAction;
    static string? sourceCodePedia;
    static List<string>? sourceCodes;
    static string? sourceCodeWhileTrue;

    public static (string sourceCodePedia, string sourceCodeActionV1, string sourceCodeActionV2, string sourceCodeActionV3, string sourceCodeVaccineAction, List<string> sourceCodes, string sourceCodeWhileTrue)
    GetSC(bool includeCondInList = true)
    {
        sourceCodeActionV1 = EmberMethods.CreateStringFromCsFile(
                   Path.GetFullPath(Path.Combine(
                       AppDomain.CurrentDomain.BaseDirectory,
                       "..", "..", "..", "..",
                       "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV1.cs"
                   ))
               );
        sourceCodeActionV2 = EmberMethods.CreateStringFromCsFile(
        Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV2.cs"
        ))
    );
        sourceCodeActionV3 = EmberMethods.CreateStringFromCsFile(
       Path.GetFullPath(Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
           "..", "..", "..", "..",
           "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV3.cs"
       ))
   );
        sourceCodeVaccineAction = EmberMethods.CreateStringFromCsFile(
          Path.GetFullPath(Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          "..", "..", "..", "..",
          "sandbox", "src", "Scripts", "ActionScripts", "VaccineScript.cs"
      ))
      );
        sourceCodePedia = EmberMethods.CreateStringFromCsFile(
       Path.GetFullPath(Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
                   "..", "..", "..", "..",
                   "sandbox", "src", "Scripts", "ConditionScripts", "PediatricCondition.cs"
               ))
           );
        sourceCodeWhileTrue = EmberMethods.CreateStringFromCsFile(
      Path.GetFullPath(Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      "..", "..", "..", "..",
      "sandbox", "src", "Scripts", "ActionScripts", "WhileTrueScript.cs"
  ))
  );
        sourceCodes = [];
        sourceCodes!.Add(sourceCodeActionV1);
        sourceCodes!.Add(sourceCodeActionV2);
        sourceCodes!.Add(sourceCodeActionV3);
        sourceCodes!.Add(sourceCodeVaccineAction);
        if (includeCondInList)
        {
            sourceCodes!.Add(sourceCodePedia);
        }

        return (sourceCodePedia, sourceCodeActionV1, sourceCodeActionV2, sourceCodeActionV3, sourceCodeVaccineAction, sourceCodes, sourceCodeWhileTrue);
    }

    internal static (LabOrder labOrder, Patient patient, ConsoleLogger logger, DataAccess testDataAccess, Vaccine vaccine) ScriptObjects()
    {
        LabOrder labOrder = new LabOrder("1", "Pediatrics");
        Patient patient = new Patient("1", "TestFirst", "TestLast", new DateTime(2010, 6, 1, 7, 47, 0), "M");   //mfu
        ConsoleLogger logger = new ConsoleLogger();
        DataAccess testDataAccess = new DataAccess();
        Vaccine vaccine = new Vaccine("Polio", 1, DateTime.UtcNow);

        return (labOrder, patient, logger, testDataAccess, vaccine);
    }
}