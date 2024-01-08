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
	// 读取文件数据, 返回true表示成功
	// buf:用户预构造用于存放数据的空间
	// nNumberOfBytesToRead:要读取的数据的大小
	// nNumberOfByteRead:实际读取到的数据的大小
	bool STDCALL DTU_Read(FILE_HANDLE handle, uint8_t *buf, uint32_t nNumberOfBytesToRead, uint32_t *nNumberOfByteRead);
	// 读取文件的帧数据, 返回true表示成功
	// buf:用户预构造用于存放数据的空间
	// nNumberOfBytesToRead:要读取的数据的大小
	// nNumberOfByteRead:实际读取到的数据的大小
	// isCompress:原始数据是否经过压缩, 返回为true则通过DTU_GetUnCompressData获取帧数据
	bool STDCALL DTU_ReadAsFrm(FILE_HANDLE handle, uint8_t *buf, uint32_t nNumberOfBytesToRead, uint32_t *nNumberOfByteRead, uint32_t *validDataSize, bool *isCompress);
	// 获取解压后的数据
	uint8_t* DTU_GetUnCompressData(FILE_HANDLE handle);
	uint32_t STDCALL DTU_Write(FILE_HANDLE handle, uint8_t *buf, uint32_t nNumberOfBytesToWrite);
	// 关闭文件句柄
	void STDCALL DTU_Close(FILE_HANDLE handle);
	// 释放资源
	void STDCALL DTU_Release(FILE_HANDLE handle);
	// 获取文件路径
	const char* STDCALL DTU_GetFilePath(FILE_HANDLE handle);
	bool STDCALL DTU_GetVer2HeaderInfo(FILE_HANDLE handle, candtu_file_info *header);
	bool STDCALL DTU_GetVer3HeaderInfo(FILE_HANDLE handle, file_head *header);
	// 设置参数, 用于写BLF文件
	void STDCALL DTU_SetParam(FILE_HANDLE handle, int flag, void *param);
	bool STDCALL DTU_GetParam(FILE_HANDLE handle, int flag, void *param);
	bool STDCALL DTU_IsFileOpened(FILE_HANDLE handle);
#define VERSION_2   0 // 版本2, can设备采用的格式
#define VERSION_3   1 // 版本3, canfd设备采用的格式
	uint8_t STDCALL DTU_GetDataType(FILE_HANDLE handle);

	//设置自定义时间片段配置
	bool STDCALL DTU_SetCustomTime(FILE_HANDLE handle, int customTime = 0,uint64_t firstFrameTime=0,uint64_t lastFrameTime=0);
#ifdef __cplusplus
};
#endif

#endif // _CANDTU_FILE_ACCESS_H_