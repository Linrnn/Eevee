chcp 65001
echo off
echo 执行期间可以挂后台，但是不建议有任何git操作
echo 开始统计仓库代码行数：
echo.
"D:\Program Files\Git\bin\bash.exe" -c "git ls-files | xargs wc -l"
echo.
pause

rem setlocal enabledelayedexpansion
rem set count=0
rem for /f "delims=" %%f in ('git ls-files') do (
rem     for /f %%l in ('powershell -Command "(Get-Content -Raw -Encoding UTF8 '%%f').Split(\"`n\").Count - 1"') do (
rem         echo %%l %%f
rem         set /a count+=%%l
rem     )
rem )
rem echo !count! total