#ifndef _CANCANFD_H_
#define _CANCANFD_H_

typedef unsigned short       uint16_t;
typedef unsigned char        uint8_t, byte;
typedef unsigned int         uint32_t;
typedef unsigned long long   uint64_t;

// VERSION3 �ļ�ͷ�ṹ,64 Bytes
typedef struct _file_head
{
	uint16_t type;       // ��������
	uint16_t ver;        // �汾
	uint8_t  mode;       // ��¼ģʽ
	uint8_t  flag;       // ��¼ģʽ��־
	uint8_t  res[2];     // ����
	uint8_t  sn[32];     // �豸���к�
	uint32_t tm_first;   // �ļ���һ֡����ʱ��UTC
	uint32_t tm_end;     // �ļ����һ֡ʱ��UTC, s
	uint32_t tm_start;   // �豸����ʱ��, s
	uint32_t tm_offs;    // ʱ��ƫ��, s
	uint32_t items;      // �ļ��ڲ�����Ŀ
	uint32_t len;        // �����򳤶�
}file_head;

typedef struct _item_head 
{
#define GZIP 0x1 << 0
	uint32_t  compress : 4; // ѹ����־
	uint32_t  encrypt  : 4; // ���ܱ�־
	uint32_t  res : 24;     // ����
	uint32_t  raw_len;      // ԭʼ�����򳤶�
	uint32_t  len;          // �����򳤶�
}item_head;

typedef struct _frame
{
	uint64_t timestamp;     // us
	uint32_t id;
	uint16_t send_type : 2; // ��������Ч, ����Ϊ0, 0:��������, 1:���η��� 2:�Է�����
	uint16_t tx : 1;        // 1:tx 2:rx
	uint16_t echo : 1;      
	uint16_t fd : 1;        // 1:canfd 2:can
	uint16_t rtr : 1;       // 1:remote 0:data frame
	uint16_t ext : 1;       // 1:extend 0:standard
	uint16_t err : 1;       // 1:error frame 0:normal frame;
	uint16_t brs : 1;       // 1:canfd���� 0:������
	uint16_t esi : 1;       // 1:�������� 0:��������
	uint16_t reserved : 5;  // ����
	uint16_t trigger : 1;  // ����λ
	uint8_t  channel;       // ͨ����, ��Ϊ-1, ����ñ����Ƿ��͸����е�ͨ��
	uint8_t  len;
	uint8_t  data[64];
}frame;

#define CAN_FRAME_SIZE		20

/** \brief define CAN message type */
typedef struct aw_can_msg {
	uint32_t      timestamp; /**< \brief timestamp  ʱ��� */
	uint32_t      id; /**< \brief id ֡ID */

	/** \brief flags ������Ϣ�ı�־
	 * -----------------------------------------------------------
	 * | 15��10                             | 9:8 controller type |
	 * -----------------------------------------------------------
	 * -----------------------------------------------------------
	 * | 7��iserrmsg |6��SEF | 5:SDF | 4��reseverd | 0:3 sendtype |
	 * -----------------------------------------------------------
	 * bit7��iserrmsg �Ƿ�Ϊ������Ϣ (1:err������Ϣ 0:normal��������)
	 * bit6��SEF ֡��ʽ (1:extended��չ֡ 0:std ��׼֡)
	 * bit5��SDF ֡���� (1:remoteԶ��֡   0:data����֡)
	 * bit4��reseverd ����(1:Tag)
	 * bit0~3����������  (sendtype:���궨��)
	 */
    	uint16_t      flags; /**< \brief msginfo ������Ϣ */
    	uint8_t		  channel;
	uint8_t       length; /**< \brief msglen ���ĳ��� */
}aw_can_msg_t;

/** \brief define CAN stander message type */
typedef struct aw_can_std_msg {
	aw_can_msg_t      can_msg;
	uint8_t           msgbuff[8];
}aw_can_std_msg_t;

// VERSION2 �ļ�ͷ�ṹ
typedef struct _candtu_file_info {
	uint16_t	type;
	uint16_t	ver;
	uint32_t	len; //�ļ���С-24(�ļ�ͷ)-������*��ͷ��С(8)
	uint64_t	time;//ms
	uint32_t    fst;
	uint32_t    lst;
}candtu_file_info;

#endif // _CANCANFD_H_