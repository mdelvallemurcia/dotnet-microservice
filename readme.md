# TODO
- HealthCheck
- Failover
- Como puedo forzar un nack? puedo publicar haciendo delay?
- Agregar librería control de Async
- e2e
- console project - Hosting environment: Production
- Timeout MassTransit
- Resilencia del http client


# Diagram

```mermaid
flowchart TD
    A[Christmas]
    B(Go shopping)
    A -->|Get money| B
    C{Let me think}
    B --> C
    D[Laptop]
    E[iPhone]
    C -->|One| D
    C -->|Two| E
    F[fa:fa-car Car]
    C -->|Three| F
```