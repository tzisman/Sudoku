# SudokuWeb

אפליקציית ווב ליצירה, פתרון והדפסה של סודוקו. הפרויקט כולל שרת ASP.NET Core, ממשק משתמש סטטי, ולוגיקת Sudoku משותפת.

## תכונות

- **סודוקו חדש** — יצירת לוח חדש לפי רמת קושי (normal, mid, easy, very easy)
- **פתרון** — פתרון לוח 9×9 שהוזן ידנית או נטען מהשרת
- **העלאת קובץ** — טעינת לוח מקובץ `.txt` / `.csv` (81 ספרות, 0 = תא ריק)
- **PDF** — הורדת לוח בודד כקובץ PDF
- **חוברת (Booklet)** — יצירת PDF עם 1–15 לוחות סודוקו
- **Swagger** — תיעוד API בזמן פיתוח

## דרישות

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## הרצה

מתוך תיקיית `SudokuWeb`:

```bash
dotnet run --project Sudoku.Server
```

או דרך Visual Studio — פתיחת `SudokuWeb.sln` והרצת פרויקט `Sudoku.Server`.

| פרופיל | כתובת |
|--------|--------|
| HTTP | http://localhost:5161 |
| HTTPS | https://localhost:7189 |
| Swagger (Development) | `/swagger` |
| ממשק המשתמש | `/` (דף הבית) |

## מבנה הפרויקט

```
SudokuWeb/
├── SudokuWeb.sln          # קובץ Solution
├── Core/
│   └── Sudoku.cs          # לוגיקת Sudoku (מנוע המשחק)
└── Sudoku.Server/         # שרת ASP.NET Core
    ├── Program.cs         # הגדרת האפליקציה והשירותים
    ├── Controllers/
    │   └── SudokuController.cs
    ├── Services/
    │   ├── SudokuService.cs   # יצירה, פתרון, פרסור קבצים
    │   └── PdfService.cs      # יצירת PDF (QuestPDF)
    ├── Models/
    │   └── SudokuModels.cs
    └── wwwroot/           # ממשק משתמש (HTML / CSS / JS)
        ├── index.html
        ├── css/site.css
        └── js/app.js
```

`Core/Sudoku.cs` מקושר לפרויקט השרת דרך `<Compile Include>` — אין פרויקט Core נפרד.

## API

בסיס הנתיב: `/api/sudoku`

| Method | נתיב | תיאור |
|--------|------|--------|
| POST | `/new` | יצירת סודוקו חדש |
| POST | `/solve` | פתרון לוח 9×9 |
| POST | `/upload` | העלאת קובץ (multipart/form-data, שדה `file`) |
| POST | `/pdf` | הורדת PDF של לוח בודד |
| POST | `/booklet` | הורדת PDF חוברת (1–15 לוחות) |

### דוגמאות

**סודוקו חדש (רמת קושי easy):**

```json
POST /api/sudoku/new
{ "difficulty": "easy" }
```

**פתרון:**

```json
POST /api/sudoku/solve
{
  "grid": [
    [5,3,0,0,7,0,0,0,0],
    ...
  ]
}
```

**חוברת:**

```json
POST /api/sudoku/booklet
{ "count": 5, "difficulty": "mid" }
```

### תגובה (`SudokuResponse`)

```json
{
  "puzzle": [[...]],
  "solution": [[...]],
  "solved": true
}
```

## רמות קושי

| ערך | תיאור |
|-----|--------|
| `null` / `normal` | לוח מלא (ברירת מחדל) |
| `mid` | רמת ביניים |
| `easy` | קל |
| `veryeasy` | קל מאוד |

## פורמט קובץ להעלאה

- 81 ספרות (0–9), לפי סדר השורות
- `0` = תא ריק
- רווחים, שורות חדשות ותווים לא-ספרתיים מתעלמים מהם
- סיומות נתמכות: `.txt`, `.csv`

## תלויות NuGet

| חבילה | שימוש |
|-------|--------|
| QuestPDF | יצירת קבצי PDF |
| Swashbuckle.AspNetCore | Swagger / OpenAPI |

## מנוע Sudoku

לוגיקת המשחק נמצאת ב-`Core/Sudoku.cs` וכוללת יצירת לוח, בדיקות תקינות, פתרון (backtracking), והגדרת רמות קושי.
