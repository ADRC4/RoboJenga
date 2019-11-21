MODULE NetModule

    VAR socketdev server_socket;
    VAR socketdev client_socket;
    PERS string IP:="127.0.0.1";

    PROC main()
        Server;
        WHILE TRUE DO
            SendPos;
            WaitTime(0.016);
        ENDWHILE
    ENDPROC

    PROC Server()
        SocketCreate server_socket;
        SocketBind server_socket,IP,1026;
        SocketListen server_socket;
        SocketAccept server_socket,client_socket;
    ERROR
        RETRY;
    ENDPROC

    PROC SendPos()
        CONST num byteCount:=24;
        VAR rawbytes sendBytes;
        VAR num floats{6};
        VAR rawbytes bytes;
        VAR jointtarget joints;

        joints:=CJointT(\TaskName:="T_ROB1");
        floats{1}:=joints.robax.rax_1;
        floats{2}:=joints.robax.rax_2;
        floats{3}:=joints.robax.rax_3;
        floats{4}:=joints.robax.rax_4;
        floats{5}:=joints.robax.rax_5;
        floats{6}:=joints.robax.rax_6;

        FOR i FROM 1 TO 6 DO
            PackRawBytes floats{i},bytes,(RawBytesLen(bytes)+1)\Float4;
        ENDFOR

        SocketSend client_socket\RawData:=bytes;
        
    ERROR
        Server;
        RETRY;
    ENDPROC

ENDMODULE