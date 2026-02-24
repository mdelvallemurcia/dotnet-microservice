namespace Models.Entity;

//[BsonCollection("projects")]
public record Project
(
    DateTime CreationDate,
    string Name
) : BaseEntity;