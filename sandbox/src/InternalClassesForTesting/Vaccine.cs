
using System.Data.Common;
using Ember.Scripting;

public interface IReadOnlyVaccince
{

}
public interface IReadOnlyVaccineContext
{
    // IReadOnlyVaccince Vaccince { get; }
    string Name { get; }
    void SetName(string name);
}
public interface IVaccineInterface
{
    int Id { get; }
    string GetName();
}

internal class Vaccine :
IReadOnlyVaccince, IVaccineInterface
{
    public string Name;
    public int Id;
    public DateTime ReleaseDate;
    int IVaccineInterface.Id => Id;
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