# Peritaje de código

## Resumen ejecutivo
El análisis del código del proyecto ASP.NET Web Forms revela varias áreas críticas que requieren atención para mejorar la seguridad, calidad, rendimiento y mantenibilidad. Se han identificado vulnerabilidades relacionadas con la seguridad de las sesiones, la validación de entradas y el manejo de transacciones. Además, se observan oportunidades para optimizar el rendimiento mediante la reducción de llamadas redundantes a la base de datos. La implementación de prácticas de codificación seguras, como el uso de parámetros SQL y la codificación de salidas, es esencial para mitigar riesgos de inyección SQL y XSS. Se recomienda priorizar las remediaciones de seguridad en los próximos 30 días, seguidas de mejoras de rendimiento y calidad en los siguientes 60 a 90 días.

## Tabla de riesgos

| ID    | Categoría   | Severidad | Probabilidad | Archivo:Línea                  | Evidencia                                                                 | Impacto                                      | Remediación                                                                 | Parche mínimo                                                                 |
|-------|-------------|-----------|--------------|--------------------------------|---------------------------------------------------------------------------|----------------------------------------------|-------------------------------------------------------------------------------|--------------------------------------------------------------------------------|
| R-001 | Seguridad   | Alta      | Alta         | Codigo/Site.Master.cs:10       | Uso de sesiones sin validación de roles activa                            | Acceso no autorizado a funciones administrativas | Validar roles de usuario antes de mostrar contenido sensible                   | `if (Session["uid"] != null && Session["role"].ToString() == "Admin") { pnlAdmin.Visible = true; } else { pnlAdmin.Visible = false; }` |
| R-002 | Rendimiento | Media     | Media        | Codigo/Class/Services/SalesService.cs:12 | Llamadas repetidas a `ProductData.GetById` dentro de un bucle             | Degradación del rendimiento en operaciones de venta | Cachear resultados de `GetById` antes del bucle                                  | `var products = items.Select(i => ProductData.GetById(i.productId)).ToDictionary(p => p.Id);` |
| R-003 | Seguridad   | Alta      | Alta         | Codigo/Class/Services/AuthServices.cs:12 | Falta de protección contra CSRF en el proceso de autenticación            | Posibilidad de ataques CSRF en el proceso de login | Implementar tokens CSRF en formularios de autenticación                         | Implementar AntiForgeryToken en el formulario de login y verificar en el servidor |
| R-004 | Seguridad   | Alta      | Alta         | Codigo/Class/Data/SalesData.cs:69 | Uso de `AddWithValue` sin especificar tipos de datos explícitos           | Riesgo de inyección SQL debido a conversiones implícitas | Usar `SqlParameter` con tipos de datos explícitos                                | `cmd.Parameters.Add("@FromUtc", SqlDbType.DateTime).Value = fromUtc;` |

## Checklist

- [ ] OWASP Top 10
- [ ] Sesiones seguras
- [x] BCrypt/hash
- [x] SQL parametrizado
- [ ] XSS/CSRF
- [ ] Manejo de errores/logs
- [ ] Secretos/keys

## Prioridades 30/60/90 días

- **30 días**: Implementar validación de roles y protección CSRF.
- **60 días**: Optimizar consultas a la base de datos y reducir llamadas redundantes.
- **90 días**: Mejorar el manejo de errores y la codificación de salidas para prevenir XSS.

## Notas para ASP.NET Web Forms + ADO.NET

- Utilizar `SqlParameter` para prevenir inyecciones SQL.
- Implementar validación server-side para todas las entradas de usuario.
- Usar `Server.HtmlEncode` para prevenir XSS.
- Asegurar que el control de roles esté correctamente implementado.
- Utilizar transacciones atómicas para operaciones críticas.
- Usar UTC para todas las operaciones de fecha y redondeo adecuado para cálculos monetarios.