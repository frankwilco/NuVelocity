using System.Text;

namespace NuVelocity.Text;

public class RawPropertyList : RawProperty
{
    public RawPropertyList(RawPropertyList parent = null,
                        string name = kDefaultName,
                        string description = kDefaultDescription)
        : base(name, description)
    {
        Parent = parent;
        Properties = new List<RawProperty>();
        Value = Properties;
    }

    public RawPropertyList Parent { get; protected set; }
    public List<RawProperty> Properties { get; protected set; }

    public static List<RawPropertyList> FromStream(Stream stream)
    {
        List<RawPropertyList> lists = new();

        using StreamReader reader = new(stream);
        RawPropertyList rootList = new();
        string className = "";
        RawPropertyList currentList = rootList;
        RawProperty lastProperty = null;

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line == null)
            {
                break;
            }
            line = line.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line == "{")
            {
                if (lastProperty != null)
                {
                    currentList.Properties.Remove(lastProperty);
                    var childList = new RawPropertyList(currentList,
                        lastProperty.Name, lastProperty.Value as string);
                    currentList.Properties.Add(childList);
                    currentList = childList;
                    lastProperty = childList;
                }
                else if (currentList == rootList)
                {
                    currentList.Name = className;
                }
            }
            else if (line == "}")
            {
                if (currentList.Parent == null)
                {
                    lists.Add(currentList);
                    className = "";
                    rootList = new();
                    currentList = rootList;
                    lastProperty = null;
                }
                else
                {
                    currentList = currentList.Parent;
                }
            }
            else if (line.Contains('='))
            {
                // TODO: special case for ascii-encoded binary
                var pair = line.Split('=');
                lastProperty = new RawProperty(pair[0], "", pair[1]);
                currentList.Properties.Add(lastProperty);
            }
            else
            {
                if (!string.IsNullOrEmpty(className))
                {
                    throw new InvalidDataException();
                }
                className = line;
            }
        }

        return lists;
    }

    public static List<RawPropertyList> FromBytes(byte[] bytes)
    {
        using MemoryStream stream = new MemoryStream(bytes);
        List<RawPropertyList> rootList = FromStream(stream);
        return rootList;
    }

    internal StringBuilder Serialize(StringBuilder builder, int depth = 0)
    {
        builder.Append('\t', depth);
        builder.AppendLine(ToString());
        builder.Append('\t', depth);
        builder.AppendLine("{");

        foreach (var property in Properties)
        {
            if (property is RawPropertyList propertyList)
            {
                propertyList.Serialize(builder, depth + 1);
                continue;
            }
            builder.Append('\t', depth + 1);
            builder.AppendLine(property.ToString());
        }

        builder.Append('\t', depth);
        builder.AppendLine("}");

        return builder;
    }

    public string Serialize()
    {
        return Serialize(new StringBuilder()).ToString();
    }

    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(Description))
        {
            return $"{Name}={Description}";
        }
        return Name;
    }
}
