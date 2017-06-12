#include<stdio.h>
#include<stdlib.h>
int main ()
{ 
  
  char aa[10] = "mamaaaa";
  int b = strlen(aa);
  printf("%d \n", b);
  
  if(aa[0] == 'm')
  {
    printf("%c", aa[1]);   
  }
}

  