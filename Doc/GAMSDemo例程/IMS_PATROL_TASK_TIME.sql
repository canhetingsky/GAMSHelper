/*
Navicat SQL Server Data Transfer

Source Server         : GAMS
Source Server Version : 140000
Source Host           : DESKTOP-36C9L6T:1433
Source Database       : lushushu
Source Schema         : dbo

Target Server Type    : SQL Server
Target Server Version : 140000
File Encoding         : 65001

Date: 2019-03-29 10:30:10
*/


-- ----------------------------
-- Table structure for [dbo].[IMS_PATROL_TASK_TIME]
-- ----------------------------
DROP TABLE [dbo].[IMS_PATROL_TASK_TIME]
GO
CREATE TABLE [dbo].[IMS_PATROL_TASK_TIME] (
[ID] bigint NULL ,
[PERSON_ID] varchar(32) NOT NULL ,
[TASK_ID] varchar(32) NOT NULL ,
[ORDER_NO] int NOT NULL ,
[SPEND_TIME] int NOT NULL ,
[START_TIME] datetime NOT NULL ,
[END_TIME] datetime NOT NULL 
)


GO

-- ----------------------------
-- Records of IMS_PATROL_TASK_TIME
-- ----------------------------
