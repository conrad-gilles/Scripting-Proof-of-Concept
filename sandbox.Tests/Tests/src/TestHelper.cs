public class TestHelper
{
    static string? sourceCodeActionV1;
    static string? sourceCodeActionV2;
    static string? sourceCodeActionV3;
    static string? sourceCodeVaccineAction;
    static string? sourceCodePedia;
    static List<string>? sourceCodes;

    public static (string sourceCodePedia, string sourceCodeActionV1, string sourceCodeActionV2, string sourceCodeActionV3, string sourceCodeVaccineAction, List<string> sourceCodes)
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
        sourceCodes = [];
        sourceCodes!.Add(sourceCodeActionV1);
        sourceCodes!.Add(sourceCodeActionV2);
        sourceCodes!.Add(sourceCodeActionV3);
        sourceCodes!.Add(sourceCodeVaccineAction);
        if (includeCondInList)
        {
            sourceCodes!.Add(sourceCodePedia);
        }

        return (sourceCodePedia, sourceCodeActionV1, sourceCodeActionV2, sourceCodeActionV3, sourceCodeVaccineAction, sourceCodes);
    }
}