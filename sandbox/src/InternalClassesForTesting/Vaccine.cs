
using System.Data.Common;

public interface IReadOnlyVaccince
{

}
public interface IReadOnlyVaccineContext
{
    IReadOnlyVaccince Vaccince { get; }
}

internal class Vaccine :
IReadOnlyVaccince
{
    public string Name;
    public int Id;
    public DateTime ReleaseDate;

    public Vaccine(string name, int id, DateTime releaseDate)
    {
        Name = name;
        Id = id;
        ReleaseDate = releaseDate;
    }

    public string GetName()
    {
        return Name;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return "Name: " + Name + " Id: " + Id + " Release Date: " + ReleaseDate;
    }

}