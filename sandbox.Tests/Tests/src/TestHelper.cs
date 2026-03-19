public class TestHelper
{
    static string? sourceCodeActionV1;
    static string? sourceCodeActionV2;
    static string? sourceCodeActionV3;
    static string? sourceCodeVaccineAction;
    static string? sourceCodePedia;
    static List<string>? sourceCodes;
    static string? sourceCodeWhileTrue;

    public static TestHelperRecord GetSC(bool includeCondInList = true)
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

        // return (sourceCodePedia, sourceCodeActionV1, sourceCodeActionV2, sourceCodeActionV3, sourceCodeVaccineAction, sourceCodes, sourceCodeWhileTrue);
        return new TestHelperRecord
        {
            sourceCodeActionV1 = sourceCodeActionV1,
            sourceCodeActionV2 = sourceCodeActionV2,
            sourceCodeActionV3 = sourceCodeActionV3,
            sourceCodePedia = sourceCodePedia,
            sourceCodes = sourceCodes,
            sourceCodeVaccineAction = sourceCodeVaccineAction,
            sourceCodeWhileTrue = sourceCodeWhileTrue
        };
    }

    internal static ObjectsRecord ScriptObjects()
    {
        LabOrder labOrder = new LabOrder("1", "Pediatrics");
        Patient patient = new Patient("1", "TestFirst", "TestLast", new DateTime(2010, 6, 1, 7, 47, 0), "M");   //mfu
        ConsoleLogger logger = new ConsoleLogger();
        DataAccess testDataAccess = new DataAccess();
        Vaccine vaccine = new Vaccine("Polio", 1, DateTime.UtcNow);

        // return (labOrder, patient, logger, testDataAccess, vaccine);
        return new ObjectsRecord
        {
            labOrder = labOrder,
            logger = logger,
            patient = patient,
            testDataAccess = testDataAccess,
            vaccine = vaccine
        };
    }
}

public record TestHelperRecord
{
    public required string sourceCodePedia { get; init; }
    public required string sourceCodeActionV1 { get; init; }
    public required string sourceCodeActionV2 { get; init; }
    public required string sourceCodeActionV3 { get; init; }
    public required string sourceCodeVaccineAction { get; init; }
    public required List<string> sourceCodes { get; init; }
    public required string sourceCodeWhileTrue { get; init; }
}
internal record ObjectsRecord
{
    public required LabOrder labOrder { get; init; }
    public required Patient patient { get; init; }
    public required ConsoleLogger logger { get; init; }
    public required DataAccess testDataAccess { get; init; }
    public required Vaccine vaccine { get; init; }
}