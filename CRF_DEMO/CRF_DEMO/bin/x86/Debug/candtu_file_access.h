#ifndef _CANDTU_FILE_ACCESS_H_
#define _CANDTU_FILE_ACCESS_H_
#include "cancanfd_item.h"

typedef uint32_t FILE_HANDLE;
#define INVALID_FILE_HANDLE ((FILE_HANDLE)-1)

#ifdef WIN32
#define STDCALL __stdcall
#else
#define STDCALL
#endif

#define TYPE_CRF     0
#define TYPE_MAT     1
#define TYPE_BLF     2
#define TYPE_OTHER   3

#define MODE_READ  1
#define MODE_WRITE 2
#define MODE_RW    3
typedef struct _FileCreatedInfo
{
	const char*  strFilePath;
	uint8_t dwDesiredAccess; // MODE_READ MODE_WRITE MODE_RW
}FileCreatedInfo, *PFileCreatedInfo;

#ifdef __cplusplus
extern "C" 
{
#endif
	FILE_HANDLE STDCALL DTU_Create(uint8_t file_type);
	bool STDCALL DTU_Open(FILE_HANDLE handle, PFileCreatedInfo info);
	uint32_t STDCALL DTU_GetFileSize(FILE_HANDLE handle);
	// ��ȡ�ļ�����, ����true��ʾ�ɹ�
	// buf:�û�Ԥ�������ڴ�����ݵĿռ�
	// nNumberOfBytesToRead:Ҫ��ȡ�����ݵĴ�С
	// nNumberOfByteRead:ʵ�ʶ�ȡ�������ݵĴ�С
	bool STDCALL DTU_Read(FILE_HANDLE handle, uint8_t *buf, uint32_t nNumberOfBytesToRead, uint32_t *nNumberOfByteRead);
	// ��ȡ�ļ���֡����, ����true��ʾ�ɹ�
	// buf:�û�Ԥ�������ڴ�����ݵĿռ�
	// nNumberOfBytesToRead:Ҫ��ȡ�����ݵĴ�С
	// nNumberOfByteRead:ʵ�ʶ�ȡ�������ݵĴ�С
	// isCompress:ԭʼ�����Ƿ񾭹�ѹ��, ����Ϊtrue��ͨ��DTU_GetUnCompressData��ȡ֡����
	bool STDCALL DTU_ReadAsFrm(FILE_HANDLE handle, uint8_t *buf, uint32_t nNumberOfBytesToRead, uint32_t *nNumberOfByteRead, uint32_t *validDataSize, bool *isCompress);
	// ��ȡ��ѹ�������
	uint8_t* DTU_GetUnCompressData(FILE_HANDLE handle);
	uint32_t STDCALL DTU_Write(FILE_HANDLE handle, uint8_t *buf, uint32_t nNumberOfBytesToWrite);
	// �ر��ļ����
	void STDCALL DTU_Close(FILE_HANDLE handle);
	// �ͷ���Դ
	void STDCALL DTU_Release(FILE_HANDLE handle);
	// ��ȡ�ļ�·��
	const char* STDCALL DTU_GetFilePath(FILE_HANDLE handle);
	bool STDCALL DTU_GetVer2HeaderInfo(FILE_HANDLE handle, candtu_file_info *header);
	bool STDCALL DTU_GetVer3HeaderInfo(FILE_HANDLE handle, file_head *header);
	// ���ò���, ����дBLF�ļ�
	void STDCALL DTU_SetParam(FILE_HANDLE handle, int flag, void *param);
	bool STDCALL DTU_GetParam(FILE_HANDLE handle, int flag, void *param);
	bool STDCALL DTU_IsFileOpened(FILE_HANDLE handle);
#define VERSION_2   0 // �汾2, can�豸���õĸ�ʽ
#define VERSION_3   1 // �汾3, canfd�豸���õĸ�ʽ
	uint8_t STDCALL DTU_GetDataType(FILE_HANDLE handle);

	//�����Զ���ʱ��Ƭ������
	bool STDCALL DTU_SetCustomTime(FILE_HANDLE handle, int customTime = 0,uint64_t firstFrameTime=0,uint64_t lastFrameTime=0);
#ifdef __cplusplus
};
#endif

#endif // _CANDTU_FILE_ACCESS_H_