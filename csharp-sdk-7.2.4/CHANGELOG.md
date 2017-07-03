**2017-01-12**

最新版本v7.2.4，适用于.NET Framework 2.0+ 和.NET Core

新增：分片上传(ResumableUploader)的uploadFile拥有更多的重载版本。

新增：上传文件支持用户自定义参数(extraParams)

* * *

**2017-01-10**

最新版本v7.2.3，适用于.NET Framework 2.0+ 和.NET Core

更新：HTTP模块现在可以加入请求Header内容。

* * *

**2017-01-09**

最新版本v7.2.0.0，适用于.NET Framework 2.0+ 和.NET Core

更新：函数命名风格统一，部分细节修改。

* * *

**2017-01-06**

最新版本v7.1.3.0，适用于.NET Framework 2.0+ 和.NET Core

新增：`Qiniu.RS`模块增加`Bucket`功能，用于获取某bucket的属性信息。

修改：.NET Framework 4.0版本Http请求管理器改回HttpWebRequest/HttpWebResponse方式。

* * *

**2017-01-05**

最新版本v7.1.2.0，适用于.NET Framework 2.0+ 和.NET Core

优化项目结构。

* * *

**2017-01-03**

最新版本v7.1.1.0，适用于.NET Framework 2.0+ 和.NET Core

修复部分细节BUG

更新CR.Date至2017

* * *

**2016-12-30**

最新版本v7.1.0.0，适用于.NET Framework 2.0+ 和.NET Core

已发布至NuGet，请搜索`Qiniu.Shared`

* * *

**2016-12-30**

最新版本v7.1.0.0，适用于.NET Framework 2.0+ 和.NET Core

优化：HTTP返回消息(`HttpResult`)的对象复制操作(`Shadow`)中，参考信息(`RefText`)被正确叠加。

* * *

**2016-12-29**

最新版本v7.1.0.0，适用于.NET Framework 2.0+ 和.NET Core

新增：HTTP请求的返回消息(`HttpResult`)中加入参考代码(`RefCode`)和参考信息(`RefInfo`)

新增：断点/分片上传(`ResumableUploader`)加入CRC32校验及重试逻辑(参见`UploadFileAdvanced`的说明)

* * *

**2016-12-27**

最新版本v7.1.0.0，适用于.NET Framework 2.0+ 和.NET Core

* * *

**2016-12-22**

最新版本v7.1.0.0，适用于.NET Framework 4.0+ 和.NET Core

新增：HTTPS支持(CDN/Fusion模块除外)

* * *

**2016-12-14**

最新版本v7.1.0.0，适用于.NET Framework 4.0+ 和.NET Core

新增：Fusion模块中增加HotLink（"时间戳防盗链"功能）

新增：UrlHelper增加URL分析（Host,Path,File,Query分拆）

* * *

**2016-12-12**

最新版本v7.1.0.0，适用于.NET Framework 4.0+ 和.NET Core

新增：增加代码文档

* * *

**2016-12-08**

最新版本v7.1.0.0，适用于.NET Framework 4.0+ 和.NET Core

新增：优化代码易读性

* * *

**2016-12-07**

最新版本v7.1.0.0，适用于.NET Framework 4.0+ 和.NET Core

新特性：.NET Framework 4.0+和.NET Core共用一套代码

新增：更新文件生命周期(BucketManager.UpdateLifecycle)

* * *

**2016-12-01**

最新版本v7.0.0.5

增加：[Fusion](http://developer.qiniu.com/article/index.html#fusion)相关功能

增加： dfop

修复：上传(分片上传)文件发生重试时，上传到空间的文件内容错误

修改：默认不使用CDN（可自行设置）；重试域名和原始上传域名保持一致

* * *


**2016-11-22**

最新版本v7.0.0.3，适用于.NET xx

（xx: 2.0/3.0/3.5/4.0/4.5/4.5.1/4.5.2/4.6/4.6.1/4.6.2）

增加：上传域名默认使用CDN（`Qiniu.Common.Config.UploadFromCDN:Boolean`）

优化：应对更多类型的网络错误（`WebException`）

优化：上传失败重试域名（`upHost`）保持不变

优化：上传分块数量（`blockCount`）计算优化


* * *


**2016-10-28**

优化：上传示例中增加`UpCompleteHandler`的说明


* * *


**2016-10-24**

增加：增加`listFiles`（获取空间文件列表）功能，更新示例及文档


* * *


**2016-10-08**

推出新版本v7


* * *


**2016-08 ~ 2016-09**

增加：`Zone`模块

增加：多机房支持（华东/华北/华南/北美）

增加：示例及说明文档