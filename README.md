Proceso automático

Ejecución del notebook audit_security.ipynb

Analiza los archivos del proyecto ubicados en la carpeta codigos/.

Detecta vulnerabilidades, errores de seguridad y malas prácticas.

Clasifica los hallazgos por nivel de severidad (Crítica, Alta, Media, Baja).

Generación de resultados

Cada análisis produce un reporte en formato .json dentro de la carpeta results/.

También se genera una gráfica comparativa (comparacion_vulnerabilidades.png) que muestra la diferencia entre versiones seguras e inseguras del código.

Validación de Quality Gate

Se ejecuta un Quality Gate automático que valida los resultados del análisis.

Solo se aprueba el código si no existen vulnerabilidades críticas o altas y los niveles medios y bajos están dentro de los límites definidos.

El resultado del gate se guarda en un archivo quality_gate_YYYYMMDD_HHMMSS.json en la carpeta results/.

Publicación de resultados

Todos los reportes y gráficos se guardan en /results/, permitiendo su revisión directa desde GitHub.

Si el Quality Gate falla, el pipeline se detiene automáticamente para evitar la integración de código inseguro.

📁 Carpetas principales
Carpeta	Descripción
codigos/	Contiene el código fuente a analizar.
results/	Almacena los reportes .json, el gráfico comparativo y los resultados del Quality Gate.
.github/workflows/	Contiene el pipeline de GitHub Actions que ejecuta todo el proceso automáticamente.
