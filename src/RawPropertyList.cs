namespace Velocity
{
    public class RawPropertyList : RawProperty
    {
        public RawPropertyList(RawPropertyList parent = null,
                            string name = kDefaultName,
                            string description = kDefaultDescription)
            : base(name, description)
        {
            Parent = parent;
            Properties = new List<RawProperty>();
        }

        public RawPropertyList Parent { get; protected set; }
        public List<RawProperty> Properties { get; protected set; }

        public static RawPropertyList FromStream(Stream stream)
        {
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
                    currentList = currentList.Parent;
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

            return rootList;
        }

        public static RawPropertyList FromBytes(byte[] bytes)
        {
            using MemoryStream stream = new MemoryStream(bytes);
            RawPropertyList rootList = FromStream(stream);
            return rootList;
        }
    }
}
