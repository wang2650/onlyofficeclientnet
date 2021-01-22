# onlyofficeclientnet
这是一个onlyoffice documentserver 的客户端 ,src下面的为net framework 。另一个文件夹为netcore5版本

##### 主要解决了官方示例的c#项目中的问题：

1 现改为目录自定义，以日期区分，同一天上传的文件放到同一文件夹下 （原版 文件默认保存在跟目前下，以ip区分。）

2 对文件的上传加入限制  （原版上传文件没有限制）

3 下载文件加入限制  （documentserver服务器，要去本地读取文件，不加限制的话，则任何人可以看到文件）

4 在postgresql 中加入新的表，用来记录文件信息。

5 orm使用了petapoco ，（本来想用sqlsugar，但是sqlsugar只有netcore版本 支持访问postgresql）

