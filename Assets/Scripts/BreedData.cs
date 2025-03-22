using System;

[Serializable]
public class BreedData
{
    public string id;
    public BreedAttributes attributes;
}

[Serializable]
public class BreedAttributes
{
    public string name;
    public string description;
}