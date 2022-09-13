namespace SimpleFactoryGenerator.SourceGenerator;

public class ProductInfo
{
    public string Label { get; set; } = null!;

    /*
     * Since the library must be in the same assembly as the product type 
     * (otherwise, the annotation cannot be referenced),
     * and the generated simple factory is also in the same assembly, 
     * the only product classes that are not accessible in the simple factory are the nested classes whose access modifier is private.
     */
    public bool IsPrivate { get; set; }

    public string ProductClassDeclaration { get; set; } = null!;
}
