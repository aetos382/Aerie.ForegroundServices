# Aerie.ForegroundServices

## これは何？
[.NET Core 汎用ホスト](https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/host/generic-host) で、メイン処理が終わったらアプリを自動的に終了させる仕組みのサンプルです。

## .NET Core 汎用ホスト
ASP.NET Core はコンソール アプリケーションで Web サーバーをホストしますが、そこから Web サーバー機能を取り除いて、汎用的なコンソール アプリケーションで、以下のような機能を使えるようにしたものが **汎用ホスト** です。

- Dependency Injection (DI)
- JSON や環境変数によるアプリケーション構成
- ロギング
- バックグラウンド サービス

なお、汎用ホストは .NET Core 2.1 からの新機能です。

## 汎用ホストに足りないもの
汎用ホストには [IHostedService インターフェイス](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.hosting.ihostedservice?view=aspnetcore-2.1) を実装したクラスや [BackgroundService クラス](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.hosting.backgroundservice?view=aspnetcore-2.1) から派生したクラスを [IServiceCollection.AddHostedService 拡張メソッド](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.dependencyinjection.servicecollectionhostedserviceextensions.addhostedservice?view=aspnetcore-2.1#Microsoft_Extensions_DependencyInjection_ServiceCollectionHostedServiceExtensions_AddHostedService__1_Microsoft_Extensions_DependencyInjection_IServiceCollection_) で登録してやることで、アプリケーションで簡単に複数のバックグラウンド サービスをホストすることができる仕組みが用意されています。

この仕組みはサーバー的な用途が想定されており、ユーザーが Ctrl+C などでホスト アプリケーションを終了させるまで、ずっと動き続けます。

ところで、コンソール アプリケーションの用途としてはもう一つ、起動したら一定の処理を行い、自動的に終了するという形態があります（というか、そっちの方がメインだと思います）。汎用ホストでは、このような形態を実現する機能は、直接的には用意されていません。

そこで作ったのがこれです。

## ホストのライフサイクル
Main メソッド内で [IHost.RunAsync 拡張メソッド](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.hosting.hostingabstractionshostextensions.runasync?view=aspnetcore-2.1) を呼ぶと、内部で [WaitForShutdownAsync メソッド](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.hosting.hostingabstractionshostextensions.waitforshutdownasync?view=aspnetcore-2.1) が呼ばれ、これによってホスト プロセスのメイン スレッドは終了待ち状態で停止します。

終了待ち状態を解除して Main の実行を再開し、プロセスを終了させるには、ワーカー スレッドから [IApplicationLifetime インターフェイス](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.hosting.iapplicationlifetime?view=aspnetcore-2.1) の [StopApplication メソッド](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.hosting.iapplicationlifetime.stopapplication?view=aspnetcore-2.1) を呼んでやる必要があります。

バックグラウンド サービスから呼んでもいいのですが、基本的にバックグラウンド サービスは、ずっと動き続ける性質のものであり、ホストの終了条件を判断することは少ないと思います。  
そのため、ホスト プロセスの寿命を司る [IHostLifetime インターフェイス](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.hosting.ihostlifetime?view=aspnetcore-2.1) というのが用意されており、これを実装したクラスが StopApplication メソッドを呼び出します。  
汎用ホストには既定で [ConsoleLifetime クラス](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.hosting.internal.consolelifetime?view=aspnetcore-2.1) が組み込まれており、Ctrl+C を押すと終了させるといった制御はこいつが行っています。

## フォアグラウンド サービス
[IForegroundService インターフェイス](src/Aerie.ForegroundServices/IForegroundService.cs) を実装したクラスを、便宜上 **フォアグラウンド サービス** と呼びます。  
これは、普通のコンソール アプリケーションのように、一定の処理を行ったら自動的に終了することを想定しています。

ConsoleLifetime の代わりに [ForegroundLifetime クラス](src/Aerie.ForegroundServices/ForegroundLifetime.cs) を組み込むことで、フォアグラウンド サービスの実行が終了したら、自動的に StopApplication を呼び出して、ホストを終了させます。

使い方は [サンプル](samples/ForegroundServiceSample) を見てください。

## 注意点
これはフォアグラウンド サービスに限らず、バックグラウンド サービスにも言えることなのですが、**サービス内で同期的にスレッドを止めてはいけません。** 最悪、他のサービスが全部止まります。

バックグラウンド サービスは（半）無限ループを回して、その中で I/O 待ちなどの非同期処理を行うことが多いので、自然と非同期になると思いますが、フォアグラウンド サービスは、シングル スレッドなコンソール アプリケーションの使用感に似せたものなので、うっかり [Console.ReadKey メソッド](https://docs.microsoft.com/ja-jp/dotnet/api/system.console.readkey?view=netcore-2.1) などで止めないようにしてください。

## ぼやき
[CancellationToken](https://docs.microsoft.com/ja-jp/dotnet/api/system.threading.cancellationtoken?redirectedfrom=MSDN&view=netcore-2.1) を [汎用的なシグナル機構として使う](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.hosting.iapplicationlifetime.applicationstarted?view=aspnetcore-2.1) のはどうなんですかね…
