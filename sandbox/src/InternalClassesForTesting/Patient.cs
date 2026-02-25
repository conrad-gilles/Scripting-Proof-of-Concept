using System;
using System.Collections.Generic;
using Ember.Scripting;

// Interface for script access (safe subset)
public interface PatientInterface
{
    string PatientId { get; }
    string FirstName { get; }
    string LastName { get; }
    DateTime? DateOfBirth { get; }
    string Gender { get; }
    int Age { get; }
    string GetCustomField(string fieldName);
}

// Full Implementation
internal class Patient : PatientInterface
{
    public string PatientId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string Gender { get; private set; } // "M", "F", "U"

    // Internal dictionary for custom fields (insurance, history, etc.)
    private Dictionary<string, string> _customFields = new Dictionary<string, string>();

    public Patient(string id, string first, string last, DateTime? dob, string gender)
    {
        PatientId = id;
        FirstName = first;
        LastName = last;
        DateOfBirth = dob;
        Gender = gender;
    }

    public int Age
    {
        get
        {
            if (!DateOfBirth.HasValue) return 0;
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Value.Year;
            if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    public string GetCustomField(string fieldName)
    {
        return _customFields.ContainsKey(fieldName) ? _customFields[fieldName] : null;
    }

    // Helper for your PoC initialization
    public void SetCustomField(string key, string value)
    {
        _customFields[key] = value;
    }
}
