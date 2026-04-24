# CSV 配置与 Excel2Cs 借鉴说明

本文档说明当前项目使用的 **CSV + CsvUtil** 配置管线，以及从 **Excel2CsTool** 可借鉴的优点及后续可落实方式。仅作说明与规划，不要求立刻改代码。

---

## 一、当前使用的管线：CSV + CsvUtil

### 1.1 数据与加载

- **数据位置**：`Assets/Resources/csv/`，如 `minion.csv`、`enemy.csv`、`skill.csv`、`building.csv`、`encounter.csv`、`chapter.csv`、`inBattleLevelUp.csv`、`mergeBuilding.csv`、`generalWave.csv`。
- **加载方式**：运行时通过 `CsvUtil.LoadObjects<T>("文件名")` 加载，内部使用 `Resources.Load<TextAsset>("csv/文件名").text`，再按首行表头 + 反射解析到 C# 对象。
- **使用方**：`CSVLoader.Init()` 统一加载上述表并填入各类字典（如 `minionCharacterInfoDict`、`skillInfoDict` 等），战斗、选关、卡牌等逻辑均依赖 CSVLoader。

### 1.2 约定

- **第一行**：表头，列名与 C# DTO 的**字段名**按列对应，**不区分大小写**。
- **第二行起**：数据行，每行一条记录。
- **以 `#` 开头的列名**：表头中若某列名为 `#xxx`（如 `#描述`、`#Description`），该列会被忽略，不参与反序列化，可用于在 CSV 内写注释/说明。
- **类型**：完全由 C# DTO 的字段类型决定，CSV 中无“类型行”。CsvUtil 支持的字段类型包括：基础类型（int、string、bool、float 等）、`List<int>` / `List<string>` / `List<float>`（CSV 中用 `|` 分隔）以及部分 Dictionary 等。

### 1.3 DTO 定义

- 所有表的 DTO 均为**手写**，集中在 `CSVLoader.cs` 及同文件中的类型定义（如 `CharacterInfo`、`SkillCsvRow`、`EnemyCsvRow`、`EncounterInfo`、`BuildingInfo` 等）。
- 部分表使用“行 DTO”再在 CSVLoader 中转换为业务结构（例如 `SkillCsvRow` → `SkillInfo`，`EnemyCsvRow` → `CharacterInfo` + `EnemyInfo`）。

---

## 二、Excel2CsTool 的定位与优点（仅供学习）

- **定位**：编辑器菜单 `SDGSupporter/Excel/Excel2Cs`，从项目外 `ExcelData/*.xlsx` 读取表结构，仅生成 C# 数据类到 `Scripts/DataTable`（namespace Table），**不生成数据、也不参与当前游戏的配置加载**。
- **优点**（可借鉴到 CSV 管线）：
  1. **显式类型行**：Excel 第 2 行为类型（如 `int`、`string`、`list<int>`），表结构自说明，便于策划/程序对齐。
  2. **描述行**：第 3 行为描述，可对应到生成类的注释，便于可读与维护。
  3. **结构单一来源**：表结构（列名 + 类型）来自 Excel，减少“改 CSV 忘改 C#”的漂移。

---

## 三、在现有 CSV 管线上可落实的借鉴点（规划）

以下为**文档约定与后续实现建议**，不要求立即改代码；若将来实现，可再按此说明修改 CsvUtil / 使用方式。

### 3.1 可选“类型行”约定（借鉴显式类型）

- **约定**：允许 CSV 在**第二行**增加一行“类型行”，与表头列一一对应，例如：  
  `int,string,string,int,float,...,list<int>,...`
- **作用**：  
  - 在 CSV 内自说明每列类型，方便策划与程序统一理解。  
  - 若将来在 CsvUtil 中实现“解析时识别并跳过类型行”，则：  
    - 现有无类型行的 CSV 行为不变；  
    - 带类型行的 CSV 在解析时自动跳过该行，不当作数据；  
  - 可选：在编辑器或加载时做“类型行与 C# DTO 字段类型一致性”校验，用于发现 CSV 与代码不同步。
- **当前**：仅作为约定与文档说明，CsvUtil 暂不解析类型行；若 CSV 第二行写成类型，目前会被当成第一条数据行，故**在未改代码前不要在实际使用的 CSV 中加类型行**，以免错位。

### 3.2 描述列（已支持，可直接用）

- 表头列名以 `#` 开头会被 CsvUtil 忽略，例如 `#描述`、`#说明`、`#备注`。
- **建议**：在需要说明的表中直接增加 `#描述` 等列，对每列或每行写注释，无需改代码即可提升可读性。

### 3.3 从 CSV 生成 DTO 骨架（可选工具）

- **思路**：借鉴 Excel2Cs“从结构生成类”的做法，可增加一个**仅用于辅助的**编辑器小工具：读取某 CSV 的表头（及可选类型行），生成一个 C# DTO 的骨架（字段名 + 类型占位），供粘贴到 CSVLoader 或单独文件中再手调。
- **价值**：新增表时减少手写 DTO 与表头不同步的概率；不强制使用，现有手写 DTO 流程可保持不变。

---

## 四、小结

| 项目         | 当前 CSV 管线                         | 从 Excel2Cs 借鉴（文档/规划）                    |
|--------------|----------------------------------------|--------------------------------------------------|
| 类型从哪里看 | 仅看 C# 代码                           | 约定可选“类型行”，未来可解析/校验                 |
| 描述/注释    | 仅 C# 或口头约定                       | 已支持 `#列名` 忽略列，可多用 `#描述` 等          |
| 结构单一来源 | CSV 表头 + 手写 DTO                    | 可选：由 CSV 表头（+类型行）生成 DTO 骨架再手调   |

Excel2CsTool 不必强制使用，其价值在于学习和参考；上述优点以**文档约定与后续可做项**的形式，在已使用的 CSV + CsvUtil 管线上落实即可。
