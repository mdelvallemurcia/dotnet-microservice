# TODO
- Como puedo forzar un nack? puedo publicar haciendo delay?
- Agregar librería control de Async
- e2e
- console project - Hosting environment: Production
- Timeout MassTransit
- Resilencia del http client
- Front
  - Agregar bearer token automáticamente
  - Forma más segura de alma¡cenar el bearer token en local
  - Logout
  - Listado de proyectos


# Diagram
```mermaid
flowchart TD
    A[☁️ Api]
    B(🐰 RabbitMq)
    C(🌿 MongoDb)
    D[⚙️ ProjectSubscriber]

    A --> B
    A --> C
    D --> B
    D --> C    
    
```