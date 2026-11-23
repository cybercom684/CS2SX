// Runtime module: binary files — File.ReadAllBytes / WriteAllBytes, streams
// (FileStream, MemoryStream) and BinaryReader / BinaryWriter in the .NET format
// (little endian; strings with a 7-bit encoded length, UTF-8).
#include "cs2sx_prelude.h"

// ── File.ReadAllBytes / WriteAllBytes ──────────────────────────────────────────

u8* CS2SX_File_ReadAllBytes(const char* path)
{
    if (!path) CS2SX_ThrowArgNull("path");
    FILE* fp = fopen(path, "rb");
    if (!fp) CS2SX_ThrowFileNotFound(path);
    fseek(fp, 0, SEEK_END);
    long size = ftell(fp);
    fseek(fp, 0, SEEK_SET);
    if (size < 0) { fclose(fp); CS2SX_ThrowIO("Could not read the file."); }
    u8* data = (u8*)CS2SX_ArrNew((int)size, 1);
    size_t got = size > 0 ? fread(data, 1, (size_t)size, fp) : 0;
    fclose(fp);
    if ((long)got != size) CS2SX_ThrowIO("Could not read the file.");
    return data;
}

static void write_bytes(const char* path, const u8* data, const char* mode)
{
    if (!path) CS2SX_ThrowArgNull("path");
    FILE* fp = fopen(path, mode);
    if (!fp) CS2SX_ThrowIO("Could not open the file for writing.");
    int n = data ? CS2SX_ArrLen(data) : 0;
    size_t put = n > 0 ? fwrite(data, 1, (size_t)n, fp) : 0;
    fclose(fp);
    if ((int)put != n) CS2SX_ThrowIO("Could not write the file.");
}

void CS2SX_File_WriteAllBytes(const char* path, const u8* data) { write_bytes(path, data, "wb"); }
void CS2SX_File_AppendAllBytes(const char* path, const u8* data) { write_bytes(path, data, "ab"); }

// ── Streams ─────────────────────────────────────────────────────────────────────
// One C type for FileStream and MemoryStream: a file (fp) or a growing buffer.

static Stream* stream_new(void)
{
    Stream* s = (Stream*)calloc(1, sizeof(Stream));
    if (!s) CS2SX_ThrowNew(&CS2SX_TI_OutOfMemoryException, NULL);
    return s;
}

static Stream* stream_open(const char* path, const char* mode, int canRead, int canWrite)
{
    if (!path) CS2SX_ThrowArgNull("path");
    FILE* fp = fopen(path, mode);
    if (!fp)
    {
        if (canRead && !canWrite) CS2SX_ThrowFileNotFound(path);
        CS2SX_ThrowIO("Could not open the file.");
    }
    Stream* s = stream_new();
    s->fp = fp;
    s->canRead = canRead;
    s->canWrite = canWrite;
    return s;
}

FileStream* CS2SX_File_OpenRead(const char* path)  { return (FileStream*)stream_open(path, "rb", 1, 0); }
FileStream* CS2SX_File_Create(const char* path)    { return (FileStream*)stream_open(path, "w+b", 1, 1); }
FileStream* CS2SX_File_OpenWrite(const char* path)
{
    // .NET: opens or creates, does not truncate, starts at 0.
    FILE* probe = fopen(path, "rb");
    if (probe) { fclose(probe); return (FileStream*)stream_open(path, "r+b", 1, 1); }
    return (FileStream*)stream_open(path, "w+b", 1, 1);
}

MemoryStream* CS2SX_MemoryStream_From(const u8* data)
{
    Stream* s = stream_new();
    s->canRead = s->canWrite = 1;
    int n = data ? CS2SX_ArrLen(data) : 0;
    if (n > 0)
    {
        s->mem = (u8*)malloc((size_t)n);
        if (!s->mem) CS2SX_ThrowNew(&CS2SX_TI_OutOfMemoryException, NULL);
        memcpy(s->mem, data, (size_t)n);
        s->len = s->cap = (size_t)n;
    }
    return (MemoryStream*)s;
}

static void stream_check(Stream* s)
{
    if (!s) CS2SX_ThrowNullRef();
    if (s->closed) CS2SX_ThrowNew(&CS2SX_TI_ObjectDisposedException, "Cannot access a closed Stream.");
}

int Stream_Read(void* vs, u8* buffer, int offset, int count)
{
    Stream* s = (Stream*)vs;
    stream_check(s);
    if (!buffer) CS2SX_ThrowArgNull("buffer");
    if (offset < 0 || count < 0 || offset + count > CS2SX_ArrLen(buffer)) CS2SX_ThrowArgOutOfRange("count");
    if (s->fp) return (int)fread(buffer + offset, 1, (size_t)count, s->fp);
    size_t left = s->pos < s->len ? s->len - s->pos : 0;
    size_t n = (size_t)count < left ? (size_t)count : left;
    memcpy(buffer + offset, s->mem + s->pos, n);
    s->pos += n;
    return (int)n;
}

int Stream_ReadByte(void* vs)
{
    Stream* s = (Stream*)vs;
    stream_check(s);
    if (s->fp) { int c = fgetc(s->fp); return c == EOF ? -1 : c; }
    return s->pos < s->len ? s->mem[s->pos++] : -1;
}

static void mem_reserve(Stream* s, size_t need)
{
    if (need <= s->cap) return;
    size_t cap = s->cap ? s->cap * 2 : 256;
    while (cap < need) cap *= 2;
    u8* m = (u8*)realloc(s->mem, cap);
    if (!m) CS2SX_ThrowNew(&CS2SX_TI_OutOfMemoryException, NULL);
    s->mem = m;
    s->cap = cap;
}

static void stream_write_raw(Stream* s, const void* data, size_t n)
{
    stream_check(s);
    if (!s->canWrite) CS2SX_ThrowNew(&CS2SX_TI_NotSupportedException, "Stream does not support writing.");
    if (n == 0) return;
    if (s->fp)
    {
        if (fwrite(data, 1, n, s->fp) != n) CS2SX_ThrowIO("Could not write the file.");
        return;
    }
    mem_reserve(s, s->pos + n);
    memcpy(s->mem + s->pos, data, n);
    s->pos += n;
    if (s->pos > s->len) s->len = s->pos;
}

void Stream_Write(void* vs, const u8* buffer, int offset, int count)
{
    Stream* s = (Stream*)vs;
    if (!buffer) CS2SX_ThrowArgNull("buffer");
    if (offset < 0 || count < 0 || offset + count > CS2SX_ArrLen(buffer)) CS2SX_ThrowArgOutOfRange("count");
    stream_write_raw((Stream*)vs, buffer + offset, (size_t)count);
}

void Stream_WriteByte(void* vs, u8 value) { stream_write_raw((Stream*)vs, &value, 1); }

long long Stream_get_Length(void* vs)
{
    Stream* s = (Stream*)vs;
    stream_check(s);
    if (!s->fp) return (long long)s->len;
    long here = ftell(s->fp);
    fseek(s->fp, 0, SEEK_END);
    long end = ftell(s->fp);
    fseek(s->fp, here, SEEK_SET);
    return end;
}

long long Stream_get_Position(void* vs)
{
    Stream* s = (Stream*)vs;
    stream_check(s);
    return s->fp ? (long long)ftell(s->fp) : (long long)s->pos;
}

void Stream_set_Position(void* vs, long long value)
{
    Stream* s = (Stream*)vs;
    stream_check(s);
    if (value < 0) CS2SX_ThrowArgOutOfRange("value");
    if (s->fp) fseek(s->fp, (long)value, SEEK_SET);
    else s->pos = (size_t)value;
}

long long Stream_Seek(void* vs, long long offset, int origin)
{
    Stream* s = (Stream*)vs;
    stream_check(s);
    long long base = origin == 1 ? Stream_get_Position(s) : origin == 2 ? Stream_get_Length(s) : 0;
    Stream_set_Position(s, base + offset);
    return base + offset;
}

void Stream_Flush(void* vs)
{
    Stream* s = (Stream*)vs;
    stream_check(s);
    if (s->fp) fflush(s->fp);
}

u8* Stream_ToArray(void* vs)
{
    Stream* s = (Stream*)vs;
    if (!s) CS2SX_ThrowNullRef();
    u8* a = (u8*)CS2SX_ArrNew((int)s->len, 1);
    if (s->len) memcpy(a, s->mem, s->len);
    return a;
}

void Stream_Close(void* vs) { Stream_Dispose(vs); }

void Stream_Dispose(void* vs)
{
    Stream* s = (Stream*)vs;
    if (!s || s->closed) return;
    if (s->fp) fclose(s->fp);
    s->fp = NULL;
    free(s->mem);
    s->mem = NULL;
    s->len = s->cap = s->pos = 0;
    s->closed = 1;   // (the struct stays: a reader may still point at it)
}

// ── BinaryReader ───────────────────────────────────────────────────────────────

BinaryReader* BinaryReader_New(void* input)
{
    if (!input) CS2SX_ThrowArgNull("input");
    BinaryReader* r = (BinaryReader*)calloc(1, sizeof(BinaryReader));
    if (!r) CS2SX_ThrowNew(&CS2SX_TI_OutOfMemoryException, NULL);
    r->f_BaseStream = (Stream*)input;
    return r;
}

static void read_exact(BinaryReader* r, void* out, size_t n)
{
    if (!r) CS2SX_ThrowNullRef();
    Stream* s = r->f_BaseStream;
    stream_check(s);
    size_t got;
    if (s->fp) got = fread(out, 1, n, s->fp);
    else
    {
        size_t left = s->pos < s->len ? s->len - s->pos : 0;
        got = n < left ? n : left;
        memcpy(out, s->mem + s->pos, got);
        s->pos += got;
    }
    if (got != n) CS2SX_ThrowNew(&CS2SX_TI_EndOfStreamException, NULL);
}

#define CS2SX_READ(Name, CType)                                         \
    CType BinaryReader_Read##Name(BinaryReader* r) { CType v; read_exact(r, &v, sizeof v); return v; }
CS2SX_READ(Byte, u8)
CS2SX_READ(SByte, s8)
CS2SX_READ(Int16, short)
CS2SX_READ(UInt16, unsigned short)
CS2SX_READ(Int32, int)
CS2SX_READ(UInt32, unsigned int)
CS2SX_READ(Int64, long long)
CS2SX_READ(UInt64, unsigned long long)
CS2SX_READ(Single, float)
CS2SX_READ(Double, double)
#undef CS2SX_READ

int BinaryReader_ReadBoolean(BinaryReader* r) { return BinaryReader_ReadByte(r) != 0; }

u8* BinaryReader_ReadBytes(BinaryReader* r, int count)
{
    if (count < 0) CS2SX_ThrowArgOutOfRange("count");
    if (!r) CS2SX_ThrowNullRef();
    Stream* s = r->f_BaseStream;
    stream_check(s);
    // .NET: fewer bytes at the end of the stream, no exception.
    u8* tmp = (u8*)malloc(count > 0 ? (size_t)count : 1);
    if (!tmp) CS2SX_ThrowNew(&CS2SX_TI_OutOfMemoryException, NULL);
    size_t got;
    if (s->fp) got = fread(tmp, 1, (size_t)count, s->fp);
    else
    {
        size_t left = s->pos < s->len ? s->len - s->pos : 0;
        got = (size_t)count < left ? (size_t)count : left;
        memcpy(tmp, s->mem + s->pos, got);
        s->pos += got;
    }
    u8* a = (u8*)CS2SX_ArrNew((int)got, 1);
    memcpy(a, tmp, got);
    free(tmp);
    return a;
}

const char* BinaryReader_ReadString(BinaryReader* r)
{
    // Length: 7 bits per byte, low bits first, high bit = more.
    int len = 0, shift = 0;
    for (;;)
    {
        u8 b = BinaryReader_ReadByte(r);
        len |= (b & 0x7F) << shift;
        if (!(b & 0x80)) break;
        shift += 7;
        if (shift > 28) CS2SX_ThrowNew(&CS2SX_TI_FormatException, "Bad string length.");
    }
    char* buf = CS2SX_StrAlloc((size_t)len);
    read_exact(r, buf, (size_t)len);
    buf[len] = '\0';
    return buf;
}

void BinaryReader_Dispose(BinaryReader* r)
{
    if (r) Stream_Dispose(r->f_BaseStream);   // .NET: closes the stream
}

// ── BinaryWriter ───────────────────────────────────────────────────────────────

BinaryWriter* BinaryWriter_New(void* output)
{
    if (!output) CS2SX_ThrowArgNull("output");
    BinaryWriter* w = (BinaryWriter*)calloc(1, sizeof(BinaryWriter));
    if (!w) CS2SX_ThrowNew(&CS2SX_TI_OutOfMemoryException, NULL);
    w->f_BaseStream = (Stream*)output;
    return w;
}

static void wr(BinaryWriter* w, const void* p, size_t n)
{
    if (!w) CS2SX_ThrowNullRef();
    stream_write_raw(w->f_BaseStream, p, n);
}

#define CS2SX_WRITE(Name, CType) \
    void BinaryWriter_Write##Name(BinaryWriter* w, CType v) { wr(w, &v, sizeof v); }
CS2SX_WRITE(Byte, u8)
CS2SX_WRITE(SByte, s8)
CS2SX_WRITE(Int16, short)
CS2SX_WRITE(UInt16, unsigned short)
CS2SX_WRITE(Int32, int)
CS2SX_WRITE(UInt32, unsigned int)
CS2SX_WRITE(Int64, long long)
CS2SX_WRITE(UInt64, unsigned long long)
CS2SX_WRITE(Single, float)
CS2SX_WRITE(Double, double)
#undef CS2SX_WRITE

void BinaryWriter_WriteBoolean(BinaryWriter* w, int v) { BinaryWriter_WriteByte(w, v ? 1 : 0); }

void BinaryWriter_WriteBytes(BinaryWriter* w, const u8* data)
{
    if (!data) CS2SX_ThrowArgNull("buffer");
    wr(w, data, (size_t)CS2SX_ArrLen(data));
}

void BinaryWriter_WriteBytesRange(BinaryWriter* w, const u8* data, int index, int count)
{
    if (!data) CS2SX_ThrowArgNull("buffer");
    if (index < 0 || count < 0 || index + count > CS2SX_ArrLen(data)) CS2SX_ThrowArgOutOfRange("count");
    wr(w, data + index, (size_t)count);
}

void BinaryWriter_WriteString(BinaryWriter* w, const char* s)
{
    if (!s) CS2SX_ThrowArgNull("value");
    size_t len = strlen(s);
    size_t v = len;
    do
    {
        u8 b = (u8)(v & 0x7F);
        v >>= 7;
        if (v) b |= 0x80;
        wr(w, &b, 1);
    } while (v);
    wr(w, s, len);
}

void BinaryWriter_Flush(BinaryWriter* w) { if (w) Stream_Flush(w->f_BaseStream); }

void BinaryWriter_Dispose(BinaryWriter* w)
{
    if (w) Stream_Dispose(w->f_BaseStream);
}
