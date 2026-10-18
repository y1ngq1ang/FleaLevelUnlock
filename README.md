# FleaLevelUnlock

SPT 5.0 服务端 mod：移除跳蚤市场的全部等级购买限制。

## 它做什么

在数据库加载完成后的 `IOnLoad` 阶段，直接修改**内存中**的数据库对象：

| 层 | 字段 | 改动 |
| --- | --- | --- |
| 全局解锁等级 | `GlobalTable.Configuration.RagFair.MinUserLevel` | `15` → `0`（1 级即可使用跳蚤市场） |
| 按类别等级总开关 | `GlobalTable.Configuration.RagFair.RagfairMinUserLevelByCategory` | `true` → `false`（`RagfairLevelService.Enabled` 读它，置 false 等于关掉整套等级锁） |
| 物品等级 | `TemplateTable.Items[*].Properties.RagfairLevelToTrade` | 正数归零（当前数据 72 个物品） |
| 分类等级 | `TemplateTable.Handbook.Categories[*].RagfairLevelToTrade` | 正数归零（当前数据 39 个分类） |

**不修改任何磁盘文件**，所以 SPT 的 `SPT_Data/checks.dat` 哈希校验照常通过（校验用 XXH3-64，改 `globals.json` / `items.json` / `handbook.json` 会让服务端直接拒绝启动）。

## 编译

```bash
dotnet build -c Release
```

默认从 `C:\SPT-5.0-BE\SPT_Runtime` 解析 SPT 程序集，并把 `FleaLevelUnlock.dll` 自动部署到
`C:\SPT-5.0-BE\SPT_Runtime\user\mods\FleaLevelUnlock\`。

SPT 装在别处时用 `SptRoot` 覆盖：

```bash
dotnet build -c Release -p:SptRoot=D:\SPT
```

只构建、不部署：`-p:DeployOnBuild=false`。

`csproj` 里所有 SPT 程序集引用都是 `Private=false`，不会把服务端 dll 一起复制进 mod 目录（mod 目录里的每个 dll 都会被加载，复制进去会冲突）。

## 验证

启动服务端后日志里应出现两行：

```
模组：FleaLevelUnlock 版本1.0.0 (GUID: com.spt.flealevelunlock ...)
[FleaLevelUnlock] Flea market level requirements removed: minUserLevel 15 -> 0, per-category levels off, 72 item(s) and 39 handbook category(ies) unlocked.
```

客户端也要重启一次（globals / items 是客户端启动时从服务端拉的）。

## 关闭 / 卸载

删掉 `SPT_Runtime/user/mods/FleaLevelUnlock/` 目录即可，数据库文件本来就是原样的。

## 已知保留项

`handbook.json` 里「子弹」(Rounds) 分类的 `RagfairLevelToTrade = -1` 没有被改动。在服务端 `RagfairLevelService.Locks()` 里负数的含义是「任何等级都锁」，属于硬性禁止交易而非等级门槛，因此按原样保留。要让子弹单发也在跳蚤市场可交易，把 `ClearCategoryLevels()` 的条件从 `is > 0` 改成 `is not null` 重新编译即可。

商人忠诚等级锁、任务锁是另外的机制，不在本 mod 范围内。
