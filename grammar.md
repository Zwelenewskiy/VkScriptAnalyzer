**GOAL** -> INSTRUCTION_LIST

**INSTRUCTION_LIST** -> 
	INSTRUCTION INSTRUCTION_LIST |
	E	

**INSTRUCTION** -> 
	var VAR
	if IF |
	while WHILE |
	return RETURN |
	ID ASSIGNMENT

**RETURN** -> EXPR ;

**IF** -> ( EXPR ) BODY ELSE

**ELSE** -> else BODY |
	E

**WHILE** -> ( EXPR ) BODY 

**BODY**-> { INSTRUCTION_LIST } |
	INSTRUCTION

**ASSIGNMENT** -> = EXPR ;

**VAR** -> ID = EXPR VAR1 ;

**VAR1** -> , ID = EXPR VAR1 |
		        E

**EXPR**  ->  T1 or EXPR |
 	         T1

**T1**   ->   T2 and T1 | 
    T2

**T2**   ->   T3 < T2 | 
    T3 > T2 | 
    T3 <= T2 | 
    T3 >= T2 | 
    T3 == T2 | 
    T3 != T2 | 
    T3

**T3**   ->   T4 + T3 |
   T4 - T3 |  
   T4
   
**T4**   ->   T5 * T4 |
    T5 / T4 |  
    T5
    
**T5**   ->   T6 . T5 |
    T6
    
**T6**** ->   NUM |
  ID |
  API CALL |
  ( CONDITION ) |
  OBJECT

**CALL** -> . ID ( OBJECT)	

**OBJECT** -> { FIELDS }

**FIELDS** -> STRING_ID : EXPR FIELDS_1 | 
E

**FIELDS_1** -> , FIELDS |
   E