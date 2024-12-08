chcp 65001
echo off
echo 执行期间可以挂后台，但是不建议有任何git操作
echo.
git reflog expire --expire-unreachable=now --all
echo 完成reflog
echo.
git gc --prune=now
echo 完成prune
echo.
pause