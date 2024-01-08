#ifndef _CANCANFD_H_
#define _CANCANFD_H_

typedef unsigned short       uint16_t;
typedef unsigned char        uint8_t, byte;
typedef unsigned int         uint32_t;
typedef unsigned long long   uint64_t;

// VERSION3 文件头结构,64 Bytes
typedef struct _file_head
{
	uint16_t type;       // 数据类型
	uint16_t ver;        // 版本
	uint8_t  mode;       // 记录模式
	uint8_t  flag;       // 记录模式标志
	uint8_t  res[2];     // 保留
	uint8_t  sn[32];     // 设备序列号
	uint32_t tm_first;   // 文件第一帧数据时间UTC
	uint32_t tm_end;     // 文件最后一帧时间UTC, s
	uint32_t tm_start;   // 设备启动时间, s
	uint32_t tm_offs;    // 时间偏移, s
	uint32_t items;      // 文件内部块数目
	uint32_t len;        // 数据域长度
}file_head;

typedef struct _item_head 
{
#define GZIP 0x1 << 0
	uint32_t  compress : 4; // 压缩标志
	uint32_t  encrypt  : 4; // 加密标志
	uint32_t  res : 24;     // 保留
	uint32_t  raw_len;      // 原始数据域长度
	uint32_t  len;          // 数据域长度
}item_head;

typedef struct _frame
{
	uint64_t timestamp;     // us
	uint32_t id;
	uint16_t send_type : 2; // 仅发送有效, 接收为0, 0:正常发送, 1:单次发送 2:自发自收
	uint16_t tx : 1;        // 1:tx 2:rx
	uint16_t echo : 1;      
	uint16_t fd : 1;        // 1:canfd 2:can
	uint16_t rtr : 1;       // 1:remote 0:data frame
	uint16_t ext : 1;       // 1:extend 0:standard
	uint16_t err : 1;       // 1:error frame 0:normal frame;
	uint16_t brs : 1;       // 1:canfd加速 0:不加速
	uint16_t esi : 1;       // 1:被动错误 0:主动错误
	uint16_t reserved : 5;  // 保留
	uint16_t trigger : 1;  // 触发位
	uint8_t  channel;       // 通道号, 若为-1, 代表该报文是发送给所有的通道
	uint8_t  len;
	uint8_t  data[64];
}frame;

#define CAN_FRAME_SIZE		20

/** \brief define CAN message type */
typedef struct aw_can_msg {
	uint32_t      timestamp; /**< \brief timestamp  时间戳 */
	uint32_t      id; /**< \brief id 帧ID */

	/** \brief flags 报文信息的标志
	 * -----------------------------------------------------------
	 * | 15：10                             | 9:8 controller type |
	 * -----------------------------------------------------------
	 * -----------------------------------------------------------
	 * | 7：iserrmsg |6：SEF | 5:SDF | 4：reseverd | 0:3 sendtype |
	 * -----------------------------------------------------------
	 * bit7：iserrmsg 是否为错误信息 (1:err错误信息 0:normal正常报文)
	 * bit6：SEF 帧格式 (1:extended扩展帧 0:std 标准帧)
	 * bit5：SDF 帧类型 (1:remote远程帧   0:data数据帧)
	 * bit4：reseverd 保留(1:Tag)
	 * bit0~3：发送类型  (sendtype:见宏定义)
	 */
    	uint16_t      flags; /**< \brief msginfo 报文信息 */
    	uint8_t		  channel;
	uint8_t       length; /**< \brief msglen 报文长度 */
}aw_can_msg_t;

/** \brief define CAN stander message type */
typedef struct aw_can_std_msg {
	aw_can_msg_t      can_msg;
	uint8_t           msgbuff[8];
}aw_can_std_msg_t;

// VERSION2 文件头结构
typedef struct _candtu_file_info {
	uint16_t	type;
	uint16_t	ver;
	uint32_t	len; //文件大小-24(文件头)-段数量*段头大小(8)
	uint64_t	time;//ms
	uint32_t    fst;
	uint32_t    lst;
}candtu_file_info;

#endif // _CANCANFD_H_