# Peritaje de código

## Resumen ejecutivo
El análisis del código fuente del proyecto ASP.NET Web Forms con ADO.NET y SQL Server revela varias áreas críticas que requieren atención inmediata para mejorar la seguridad, calidad, rendimiento y mantenibilidad del sistema. Se identificaron riesgos significativos relacionados con la seguridad de las sesiones, el manejo de errores y la validación de entradas. Además, se observó un uso ineficiente de recursos en ciertas operaciones de base de datos que podrían afectar el rendimiento. La implementación de prácticas de codificación seguras y eficientes es esencial para mitigar estos riesgos y garantizar la robustez del sistema.

## Tabla de riesgos
| ID    | Categoría    | Severidad | Probabilidad | Archivo:Línea                  | Evidencia                                                                 | Impacto                                   | Remediación                                                                 | Parche mínimo                                                                 |
|-------|--------------|-----------|--------------|--------------------------------|---------------------------------------------------------------------------|-------------------------------------------|------------------------------------------------------------------------------|-------------------------------------------------------------------------------|
| R-001 | Seguridad    | Alta      | Alta         | Codigo/Site.Master.cs:12       | Uso de sesiones sin validación de roles adecuada                          | Acceso no autorizado a funcionalidades    | Implementar validación de roles en el servidor                               | `if (Session["uid"] != null && Session["role"] != null) { pnlAdmin.Visible = Session["role"].ToString() == "Admin"; } else { pnlAdmin.Visible = false; }` |
| R-002 | Rendimiento  | Media     | Media        | Codigo/Class/Services/SalesService.cs:15 | Llamadas repetidas a `ProductData.GetById` dentro de un bucle             | Degradación del rendimiento               | Almacenar los productos en una lista antes de calcular totales               | `var products = ProductData.GetByIds(items.Select(i => i.productId));`                                           |
| R-003 | Seguridad    | Alta      | Alta         | Codigo/Class/Services/AuthServices.cs:12 | Falta de protección contra ataques de fuerza bruta en el inicio de sesión | Compromiso de cuentas de usuario          | Implementar bloqueo de cuenta tras múltiples intentos fallidos               | `// Implementar lógica de bloqueo de cuenta`                                                                      |
| R-004 | Seguridad    | Alta      | Alta         | Codigo/Class/Data/UserData.cs:53 | Falta de validación de entrada en el método `GetByEmail`                  | Inyección SQL                             | Validar y sanitizar entradas del usuario                                     | `if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email no puede estar vacío.");`               |

## Checklist
- [ ] OWASP Top 10
- [ ] Manejo seguro de sesiones
- [x] Uso de BCrypt para hashing de contraseñas
- [x] SQL parametrizado
- [ ] Protección contra XSS/CSRF
- [ ] Manejo adecuado de errores y logs
- [ ] Protección de secretos y claves

## Prioridades 30/60/90 días
- **30 días**: Implementar validación de roles y protección contra ataques de fuerza bruta.
- **60 días**: Optimizar el rendimiento de las consultas a la base de datos.
- **90 días**: Mejorar el manejo de errores y la protección contra XSS/CSRF.

## Notas para ASP.NET Web Forms + ADO.NET
- Utilizar `SqlParameter` para todas las consultas SQL para prevenir inyecciones.
- Implementar validación server-side para todas las entradas del usuario.
- Usar `Server.HtmlEncode` para prevenir XSS.
- Implementar control de roles en el servidor para proteger recursos sensibles.
- Asegurar que todas las transacciones sean atómicas.
- Utilizar UTC para todas las operaciones de fecha y hora.
- Redondear operaciones monetarias a dos decimales para evitar errores de precisión.