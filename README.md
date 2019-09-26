[TOC]

# GAMSHelper

> 以下所说任务均指根据井组合并后的任务，命名规则：井组-优先级，如AS0-1、AS1-1

## 数据库

数据进行预处理

```c#
int count = 14;
List<string>[] listData = new List<string>[count];

listData[0] = Pi1;  //值班人员表：人员ID
listData[1] = Pi2;  //值班人员表：技能等级

listData[2] = TLi1; //合并后的任务ID
listData[3] = TLi2; //合并后的所需人员最低技能等级

listData[4] = Tij1; //人员ID
listData[5] = Tij2; //合并后的任务ID
listData[6] = Tij3; //完成任务的时间

listData[7] = Tjj1; //合并后的任务ID
listData[8] = Tjj2; //合并后的任务ID
listData[9] = Tjj3; //合并后的任务ID

listData[10] = TLi3; //合并后的任务优先级

listData[11] = name;    //所有井组（无重复）
listData[12] = pointName;//新的任务号
listData[13] = oldTask;  //合并任务号对应的原来的任务，任务间以 | 分隔
```

## 模型相关

### sets

- i，人员，来源值班人员表，
- j，任务

```
取优先级为0、1、2的任务。
没2有3，添加3的任务号
```

- j1，任务

```
从 j 中取任务优先级为 1 的
```

- j2，任务

```
从 j 中取任务优先级为 2 的
```

- j3，任务

```
从 j 中取任务优先级为 3 的（没2有3的情况）
```

- j4，任务

```
从 j 中取优先级 1 2
```

### Parameters

- PL，人员-技能等级，来源值班人员表
- TL，任务-所需等级，

```
取 j 中的任务，任务优先级为0、1、3的直接用
处理任务等级为2的
有2没3，任务号为2，所需等级取2的
有2有3，任务号为2，所需等级取最大的（等级高）
```

- Tij_lo，人员-任务-时间

```
取listData 4、5、6，先添加任务优先级为0、1、2的
根据井组遍历，考虑一下情况
没2有3，任务号3，所需时间为10
```

- Tij_up，人员-任务-时间

```
取listData 4、5、6，先添加任务优先级为0、1的
根据井组遍历，处理任务等级为2的
没2有3，任务号为3，时间取3的
有2没3，任务号为2，时间取2的
有2有3，任务号为2，时间取2、3的和
```

- Tjj，任务-任务-时间

```
取j中的任务，
相同井组时间为0，不同井组从数据库查询时间。
```

## 模型选择



## 求解结果

从 GAMS 求解结果中取 `Ts、Tf、XS`，后面会用到。

```c#
List<string>[] resultKeys = new List<string>[9];

resultKeys[0] = Ts_Pid;
resultKeys[1] = Ts_id;
resultKeys[2] = Ts_time;

resultKeys[3] = Tf_Pid;
resultKeys[4] = Tf_id;
resultKeys[5] = Tf_time;

resultKeys[6] = XS_Pid;
resultKeys[7] = XS_Tid;
resultKeys[8] = XS_id;
```

根据 `Ts、Tf `，向数据库插入 `GAMSresult` 表。

```
 Ts 包括 Ts_Pid、Ts_id、Ts_time，人员ID、时间段编号、起始时间
 Tf 包括 Tf_Pid、Tf_id、Tf_time，人员ID、时间段编号、结束时间
 然后向数据库插入人员ID、时间段编号、起始时间、结束时间
```

## 重构关联任务

0、1不处理，2、3的需要重构

```
根据井组遍历，处理任务等级为2的
有2有3，新任务为2，原始任务为2、3的叠加
没2有3，新任务为3，不处理
有2没3，不处理
没2没3，不处理
```

## 结果处理

根据 `XS` 得到人员、任务、order

根据 `GAMSresult` 得到花费时间、起止时间、结束时间（根据人员、order字段查询）

> order-1，奇数，任务之间迁移时间
>
> order，   偶数，完成任务时间
>
> 起始时间是（order-1）的起始时间，结束时间是 order 的结束时间

```
List<string>[] listData = new List<string>[7];
listData[0] = PERSON_ID;
listData[1] = TASK_ID;
listData[2] = ORDER_NO;
listData[3] = SPEND_TIME;
listData[4] = START_TIME;
listData[5] = BETWEEN_TIME;
listData[6] = END_TIME;
```

