# TODO
- Como puedo forzar un nack? puedo publicar haciendo delay?
- Agregar librería control de Async
- e2e
- console project - Hosting environment: Production
- Timeout MassTransit
- Resilencia del http client
- Logger using static methods
- auth-token no guardado con localStorage!!!! (activar Content-Security-Policy??)  
- Front
  - Agregar bearer token automáticamente
  - Forma más segura de alma¡cenar el bearer token en local
  - Logout
  - Listado de proyectos
  - Configuración general del proyecto: url del api, url por defecto el logearse, config del grid...
  - Importar de una todas las páginas/componentes de un directorio

# Diagram
```mermaid
flowchart LR
    WEB[💻 Web]
    API[☁️ Api]
    PROJECT_SUBSCRIBER[⚙️ ProjectSubscriber]
    RABBIT(🐰 RabbitMq)
    MONGO(🌿 MongoDb)

    API --> RABBIT
    API --> MONGO
    RABBIT --> PROJECT_SUBSCRIBER
    PROJECT_SUBSCRIBER --> MONGO
    WEB --> API
```
