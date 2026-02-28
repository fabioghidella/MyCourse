namespace MyCourse.Models.ValueObjects;

//This class is used solely to indicate to the SqliteAccessor infrastructure service
//that a given parameter should not be converted to a SqliteParameter
public class Sql
{
    private Sql(string value)
    {
        Value = value;
    }
    //Property to store the original value
    public string Value { get; }

    //Conversion to/from the string type
    public static explicit operator Sql(string value) => new Sql(value);
    public override string ToString()
    {
        return this.Value;
    }
}
